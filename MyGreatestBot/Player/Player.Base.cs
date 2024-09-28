using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    /// <summary>
    /// Player handler class
    /// </summary>
    internal sealed partial class PlayerHandler
    {
        private const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;

        private const int TRACK_LOADING_DELAY_MS = 20000;
        private const int TRACK_LOADING_WARN_RETRIES = 3;
        private const int TRACK_LOADING_FAULT_RETRIES = 6;

        private static readonly TimeSpan MaxTrackDuration = TimeSpan.FromHours(101);
        private static readonly TimeSpan MinTrackDuration = TimeSpan.FromSeconds(3);

        internal static int TransmitSinkDelay => TRANSMIT_SINK_MS;
        internal static int BufferSize => BUFFER_SIZE * 2;

        internal static Semaphore DbSemaphore { get; } = new(1, 1);

        private volatile BaseTrackInfo? currentTrack;
        private volatile PlayerStatus _status = PlayerStatus.Init;

        private volatile bool IsPlaying;
        private volatile bool IsPaused;
        private volatile bool RewindRequested;
        private volatile bool StopRequested;

        private TimeSpan PlayerTimePosition;

        private TimeSpan TimeRemaining => ((currentTrack == null || currentTrack.IsLiveStream)
            ? PlayerTimePosition
            : currentTrack.Duration) - PlayerTimePosition;

        private PlayerStatus Status
        {
            get => _status;
            set
            {
                if (value == _status)
                {
                    return;
                }
                _status = value;
                Handler.Log.Send($"{nameof(PlayerStatus)} {value}", LogLevel.Debug);
            }
        }

        private ConnectionHandler Handler { get; }

        private FFMPEG FfmpegInstance { get; }

        private readonly Task MainPlayerTask;
        private readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private readonly CancellationToken MainPlayerCancellationToken;

        private readonly Queue<BaseTrackInfo?> tracksQueue = new();

        private readonly byte[] PlayerByteBuffer = new byte[BufferSize];

        private readonly object queueLock = new();
        private readonly object trackLock = new();

        private bool disposed;

        /// <summary>
        /// Default class constructor.
        /// </summary>
        /// 
        /// <param name="handler">
        /// Referecned <see cref="ConnectionHandler"/> instance.
        /// </param>
        /// 
        /// <exception cref="PlayerException"></exception>
        internal PlayerHandler(ConnectionHandler handler)
        {
            Handler = handler;
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            try
            {
                MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
                if (!FFMPEG.CheckForExecutableExists())
                {
                    throw new FileNotFoundException($"{nameof(FFMPEG)} executable file not found");
                }
            }
            catch (Exception ex)
            {
                throw new PlayerException("Cannot start player", ex);
            }
            FfmpegInstance = new(Handler.GuildName);
        }

        /// <summary>
        /// Main player method.
        /// </summary>
        private void PlayerTaskFunction()
        {
            try
            {
                Thread.CurrentThread.Name = $"{nameof(PlayerTaskFunction)} \"{Handler.GuildName}\"";
            }
            catch { }

            Thread.CurrentThread.SetHighestAvailableTheadPriority(
                ThreadPriority.Highest,
                ThreadPriority.AboveNormal);

            while (true)
            {
                // if player termination requested
                if (MainPlayerCancellationToken.IsCancellationRequested)
                {
                    Status = PlayerStatus.Deinit;
                    break;
                }

                // await for enqueued tracks
                if (tracksQueue.Count == 0)
                {
                    Status = PlayerStatus.Idle;
                    Wait();
                    continue;
                }

                try
                {
                    // ensure voice channel connection
                    Task.Run(Handler.Voice.WaitForConnectionAsync, MainPlayerCancellationToken).Wait();

                    // dequeue track
                    if (currentTrack is null)
                    {
                        Dequeue();
                    }

                    // start processing
                    if (currentTrack is not null)
                    {
                        Status = PlayerStatus.Start;
                        PlayBody();
                        Status = PlayerStatus.Finish;
                    }
                }
                catch (TaskCanceledException)
                {
                    // player termitation
                    Status = PlayerStatus.Deinit;
                    break;
                }
                catch (TypeInitializationException ex)
                {
                    // player or/and ffmpeg not initialized
                    Status = PlayerStatus.Error;
                    Reset();
                    Handler.LogError.Send(ex.GetExtendedMessage());
                    Environment.Exit(1);
                    break;
                }
                catch (Exception ex)
                {
                    // other unhandled exceptions
                    Status = PlayerStatus.Error;
                    Reset();
                    Handler.LogError.Send(ex.GetExtendedMessage());
                }

                lock (queueLock)
                {
                    Wait();
                }

                if (StopRequested)
                {
                    Reset();
                    continue;
                }

                if (tracksQueue.Count == 0)
                {
                    Handler.Message.Send(new PlayerException("No more tracks"));
                }
            }

            Reset();
        }

        private void PlayBody()
        {
            if (currentTrack == null
                || Handler.VoiceConnection == null)
            {
                return;
            }

            uint load_retries = 0;
            bool obtain_audio = true;

            try
            {
                Handler.Voice.UpdateSink();
            }
            catch (Exception ex)
            {
                Handler.LogError.Send(
                    string.Join(Environment.NewLine,
                        "Error updating voice sink",
                        ex.GetExtendedMessage()));
            }

            Handler.Log.Send(currentTrack.GetMessage("Playing", shortMessage: true));

            IsPlaying = true;

            while (true)
            {
                Status = PlayerStatus.Obtaining;

                if (load_retries >= TRACK_LOADING_FAULT_RETRIES)
                {
                    Handler.Message.Send(new PlayerException("Cannot load track"));
                    break;
                }
                else if (load_retries > 0)
                {
                    Handler.LogError.Send($"Track load failed for {load_retries} times.",
                        load_retries < TRACK_LOADING_WARN_RETRIES ? LogLevel.Warning : LogLevel.Error);
                }

                if (obtain_audio && !ObtainBody())
                {
                    break;
                }

                Status = PlayerStatus.Loading;

                Wait(100);

                currentTrack.PerformRewind(PlayerTimePosition);

                FfmpegInstance.Start(currentTrack);

                LowPlayerResult lowPlayerResult = LowPlayerResult.NotStarted;
                ReadBytesResult readBytesResult = ReadBytesResult.NotStarted;

                // wait for bytes in ffmpeg output
                if (!FfmpegInstance.TryLoad(TRACK_LOADING_DELAY_MS))
                {
                    Wait(10);

                    PrintErrorMessage(lowPlayerResult, readBytesResult);

                    if ((!currentTrack.IsLiveStream && TimeRemaining < MinTrackDuration) || !IsPlaying)
                    {
                        break;
                    }

                    load_retries++;
                    obtain_audio = true;

                    continue;
                }

                obtain_audio = false;
                load_retries = 0;

                Handler.Voice.SendSpeaking(true);

                lowPlayerResult = LowPlayer(out readBytesResult);

                if (lowPlayerResult != LowPlayerResult.Success && !StopRequested)
                {
                    Wait(10);

                    PrintErrorMessage(lowPlayerResult, readBytesResult);

                    continue;
                }
                else
                {
                    Handler.Voice.SendSpeaking(false);
                    break;
                }
            }

            IsPlaying = false;

            FfmpegInstance.Stop();

            currentTrack = null;
        }

        private bool ObtainBody()
        {
            if (currentTrack == null)
            {
                return false;
            }

            uint obtain_retries = 0;
            bool obtain_success = false;

            while (true)
            {
                if (MainPlayerCancellationToken.IsCancellationRequested)
                {
                    break;
                }
                if (obtain_retries >= TRACK_LOADING_FAULT_RETRIES)
                {
                    Handler.Message.Send(new PlayerException("Cannot obtain audio URL"));
                    break;
                }
                else if (obtain_retries > 0)
                {
                    Handler.LogError.Send($"Obtain audio URL failed for {obtain_retries} times.",
                        obtain_retries < TRACK_LOADING_WARN_RETRIES ? LogLevel.Warning : LogLevel.Error);
                }

                try
                {
                    PlayerTimePosition = currentTrack.TimePosition;
                    currentTrack.ObtainAudioURL(TRACK_LOADING_DELAY_MS);
                    obtain_success = !string.IsNullOrWhiteSpace(currentTrack.AudioURL);
                }
                catch (Exception ex)
                {
                    obtain_success = false;
                    obtain_retries = TRACK_LOADING_FAULT_RETRIES;
                    Handler.LogError.Send(ex.GetExtendedMessage());
                    continue;
                }

                if (obtain_success)
                {
                    if (!IAccessible.IsUrlSuccess(currentTrack.AudioURL, false))
                    {
                        obtain_success = false;
                        Handler.LogError.Send("Audio URL is not available");
                        currentTrack.Reload();
                    }
                }
                else
                {
                    Handler.LogError.Send("Obtain audio URL timeout or it is empty");
                }

                if (obtain_success)
                {
                    break;
                }

                obtain_retries++;
            }

            return obtain_success;
        }

        private LowPlayerResult LowPlayer(out ReadBytesResult readBytesResult)
        {
            readBytesResult = ReadBytesResult.NotStarted;

            if (currentTrack == null)
            {
                return LowPlayerResult.TrackNull;
            }

            while (true)
            {
                // player is paused
                if (IsPaused && IsPlaying && !RewindRequested)
                {
                    Status = PlayerStatus.Paused;
                    Wait();
                    continue;
                }

                // player stop
                if (!IsPlaying || StopRequested)
                {
                    return LowPlayerResult.Success;
                }

                // player rewind
                if (RewindRequested)
                {
                    RewindRequested = false;
                    IsPaused = false;
                    PlayerTimePosition = currentTrack.TimePosition;
                    return LowPlayerResult.RestartSeek;
                }

                currentTrack.PerformRewind(PlayerTimePosition);

                readBytesResult = PerformRead(PlayerByteBuffer, out int cnt);

                if (readBytesResult != ReadBytesResult.Success)
                {
                    if (!currentTrack.IsLiveStream && TimeRemaining < MinTrackDuration)
                    {
                        // track almost ended
                        return LowPlayerResult.Success;
                    }

                    // restart ffmpeg
                    return LowPlayerResult.RestartRead;
                }

                if (!currentTrack.IsLiveStream && TimeRemaining < TimeSpan.FromMilliseconds(TRANSMIT_SINK_MS))
                {
                    // track should be ended
                    return LowPlayerResult.Success;
                }

                using CancellationTokenSource cts = new();
                using Task<int> writeTask = Handler.Voice.WriteAsync(PlayerByteBuffer, cnt, cts.Token);

                // when discord voice server changed
                // needs to be handled more propertly
                if (!writeTask.Wait(TRANSMIT_SINK_MS * 100))
                {
                    cts.Cancel();
                    try
                    {
                        writeTask.Wait();
                    }
                    catch { }
                    Handler.Voice.UpdateVoiceConnection();
                    Handler.Voice.UpdateSink();
                    return LowPlayerResult.RestartWrite;
                }

                if (writeTask.Result < 0)
                {
                    IsPaused = true;
                    continue;
                }

                PlayerTimePosition += TimeSpan.FromMilliseconds(TRANSMIT_SINK_MS * 2 * ((double)writeTask.Result / BUFFER_SIZE));

                Status = PlayerStatus.Playing;
            }
        }

        /// <summary>
        /// Reads bytes from ffmpeg.
        /// </summary>
        /// <param name="buffer">
        /// <inheritdoc cref="FFMPEG.ReadAsync(byte[], int, int, CancellationToken)" path="/param[@name='buffer']"/>
        /// </param>
        /// <param name="bytesReadCount">
        /// Number of bytes received from the stream and written to the buffer.
        /// </param>
        /// <returns>
        /// Read operation resulting status.
        /// </returns>
        private ReadBytesResult PerformRead(byte[] buffer, out int bytesReadCount)
        {
            bytesReadCount = 0;
            CancellationTokenSource cts = new();
            Task<int> read_task = FfmpegInstance.ReadAsync(buffer, 0, buffer.Length, cts.Token);
            if (read_task == null)
            {
                return ReadBytesResult.TaskNull;
            }
            if (!read_task.Wait(TRANSMIT_SINK_MS * 100))
            {
                try
                {
                    cts.Cancel();
                }
                catch { }
                Wait();
                try
                {
                    if (!read_task.IsCompleted)
                    {
                        read_task.Wait();
                    }
                }
                catch { }
                return ReadBytesResult.Timeout;
            }
            else
            {
                bytesReadCount = read_task.Result;
            }
            return bytesReadCount == 0 ? ReadBytesResult.ZeroLength : ReadBytesResult.Success;
        }
    }
}
