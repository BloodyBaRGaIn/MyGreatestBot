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
    internal sealed partial class Player
    {
        private const int TRACK_LOADING_DELAY_MS = 10000;
        private const int TRACK_LOADING_WARN_RETRIES = 3;
        private const int TRACK_LOADING_FAULT_RETRIES = 6;

        private const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;
        private const int FRAMES_TO_MS = TRANSMIT_SINK_MS * 2;
        private static readonly TimeSpan MaxTrackDuration = TimeSpan.FromHours(101);
        private static readonly TimeSpan MinTrackDuration = TimeSpan.FromSeconds(3);

        internal static int TransmitSinkDelay => TRANSMIT_SINK_MS;

        internal static Semaphore DbSemaphore { get; } = new(1, 1);

        private volatile ITrackInfo? currentTrack;
        private volatile PlayerStatus Status = PlayerStatus.Init;

        private volatile bool IsPlaying;
        private volatile bool IsPaused;
        private volatile bool SeekRequested;
        private volatile bool StopRequested;

        private TimeSpan PlayerTimePosition;

        private TimeSpan TimeRemaining => (currentTrack == null ? PlayerTimePosition : currentTrack.Duration) - PlayerTimePosition;

        private ConnectionHandler Handler { get; }

        private FFMPEG FfmpegInstance { get; }

        private readonly Task MainPlayerTask;
        private readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private readonly CancellationToken MainPlayerCancellationToken;

        private readonly Queue<ITrackInfo?> tracksQueue = new();

        private readonly byte[] PlayerByteBuffer = new byte[BUFFER_SIZE * 2];

        private readonly object queueLock = new();
        private readonly object trackLock = new();

        private enum LowPlayerResult : int
        {
            TrackNull = -1,
            Success = 0,
            RestartSeek,
            RestartRead,
            RestartWrite
        }

        private enum ReadBytesResult : int
        {
            TaskNull = -1,
            Success = 0,
            NotStarted,
            ZeroLength,
            Timeout
        }

        [Flags]
        private enum PlayerStatus : uint
        {
            None = 0x0000U,
            Init = 0x0001U,
            Idle = 0x0002U,
            InitOrIdle = Init | Idle,
            Start = 0x0004U,
            Loading = 0x0008U,
            Playing = 0x0010U,
            Paused = 0x0020U,
            Finish = 0x0040U,
            Deinit = 0x0080U,
            Error = 0x0100U,
            DeinitOrError = Deinit | Error
        }

        /// <summary>
        /// Default class constructor.
        /// </summary>
        /// 
        /// <param name="handler">
        /// Referecned <see cref="ConnectionHandler"/> instance.
        /// </param>
        /// 
        /// <exception cref="PlayerException"></exception>
        internal Player(ConnectionHandler handler)
        {
            Handler = handler;
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            try
            {
                MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
                if (!FFMPEG.CheckForExecutableExists())
                {
                    throw new FileNotFoundException(
                        string.Join(Environment.NewLine,
                            $"{nameof(FFMPEG)} executable file not found",
                            "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip"));
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
        private async void PlayerTaskFunction()
        {
            try
            {
                Thread.CurrentThread.Name = $"{nameof(PlayerTaskFunction)} {Handler.GuildName}";
            }
            catch { }

            Thread.CurrentThread.SetHighestAvailableTheadPriority(
                ThreadPriority.Highest,
                ThreadPriority.AboveNormal);

            while (true)
            {
                if (MainPlayerCancellationToken.IsCancellationRequested)
                {
                    Status = PlayerStatus.Deinit;
                    break;
                }

                if (tracksQueue.Count == 0)
                {
                    Status = PlayerStatus.Idle;
                    Wait();
                    continue;
                }

                try
                {
                    Task.Run(Handler.Voice.WaitForConnectionAsync, MainPlayerCancellationToken).Wait();

                    if (currentTrack is null)
                    {
                        Dequeue();
                    }

                    if (currentTrack is not null)
                    {
                        Status = PlayerStatus.Start;
                        PlayBody();
                        Status = PlayerStatus.Finish;
                    }
                }
                catch (TaskCanceledException)
                {
                    Status = PlayerStatus.Deinit;
                    break;
                }
                catch (TypeInitializationException ex)
                {
                    Status = PlayerStatus.Error;
                    Reset();
                    await Handler.LogError.SendAsync(ex.GetExtendedMessage());
                    Environment.Exit(1);
                    break;
                }
                catch (Exception ex)
                {
                    Status = PlayerStatus.Error;
                    Reset();
                    await Handler.LogError.SendAsync(ex.GetExtendedMessage());
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

        private bool TryObtainAudio()
        {
            if (currentTrack is null)
            {
                return false;
            }

            PlayerTimePosition = currentTrack.TimePosition;

            try
            {
                currentTrack.ObtainAudioURL();
            }
            catch (Exception ex)
            {
                Handler.Message.Send(ex);
                Handler.LogError.Send(ex.GetExtendedMessage());
                return false;
            }

            return true;
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
            catch { }

            Handler.Log.Send(currentTrack.GetShortMessage("Playing"));

            IsPlaying = true;

            while (true)
            {
                Status = PlayerStatus.Loading;

                if (load_retries >= TRACK_LOADING_FAULT_RETRIES)
                {
                    Handler.LogError.Send("Cannot load track");
                }
                else if (load_retries > 0)
                {
                    Handler.LogError.Send($"Track load failed for {load_retries} times.",
                        load_retries < TRACK_LOADING_WARN_RETRIES ? LogLevel.Warning : LogLevel.Error);
                }

                Wait(100);

                if (obtain_audio)
                {
                    if (TryObtainAudio())
                    {
                        if (!IAccessible.IsUrlSuccess(currentTrack.AudioURL, false))
                        {
                            Handler.LogError.Send("Audio URL is not available");
                            currentTrack.Reload();
                            continue;
                        }
                    }
                    else
                    {
                        Handler.LogError.Send("Cannot obtain audio URL");
                        break;
                    }
                }

                currentTrack.PerformSeek(PlayerTimePosition);

                FfmpegInstance.Start(currentTrack);

                Handler.Log.Send($"Load {nameof(FFMPEG)}", LogLevel.Debug);

                if (!FfmpegInstance.TryLoad(TRACK_LOADING_DELAY_MS))
                {
                    Wait(1);

                    if ((!currentTrack.IsLiveStream && TimeRemaining < MinTrackDuration) || !IsPlaying)
                    {
                        break;
                    }
                    load_retries++;
                    obtain_audio = true;
                    continue;
                }

                Handler.Log.Send($"Start {nameof(FFMPEG)}", LogLevel.Debug);

                Handler.Voice.SendSpeaking(true);

                LowPlayerResult lowPlayerResult = LowPlayer(out ReadBytesResult readBytesResult);

                if (lowPlayerResult != LowPlayerResult.Success && !StopRequested)
                {
                    Handler.Log.Send(
                        Environment.NewLine +
                        string.Join(Environment.NewLine,
                            $"{nameof(FFMPEG.ExitCode)} {FfmpegInstance.ExitCode}",
                            $"{nameof(LowPlayerResult)} {lowPlayerResult}",
                            $"{nameof(ReadBytesResult)} {readBytesResult}",
                            $"{nameof(PlayerTimePosition)} {ITrackInfo.GetCustomTime(PlayerTimePosition, true)}",
                            $"{nameof(TimeRemaining)} {ITrackInfo.GetCustomTime(TimeRemaining, true)}") +
                        Environment.NewLine,
                        LogLevel.Debug);

                    string errorMessage = FfmpegInstance.GetErrorMessage();

                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        Handler.Log.Send("Unexpected process exit", LogLevel.Debug);
                    }
                    else
                    {
                        Handler.Log.Send(
                            Environment.NewLine +
                            string.Join(Environment.NewLine,
                                $"Error message begin{Environment.NewLine}",
                                errorMessage,
                                "Error message end") +
                            Environment.NewLine, LogLevel.Debug);
                    }

                    obtain_audio = false;
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

            Handler.Log.Send($"Stop {nameof(FFMPEG)}", LogLevel.Debug);

            currentTrack = null;
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
                while (IsPaused && IsPlaying && !SeekRequested)
                {
                    Status = PlayerStatus.Paused;
                    Wait();
                }

                if (!IsPlaying || StopRequested)
                {
                    return LowPlayerResult.Success;
                }

                if (SeekRequested)
                {
                    SeekRequested = false;
                    IsPaused = false;
                    PlayerTimePosition = currentTrack.TimePosition;
                    return LowPlayerResult.RestartSeek;
                }

                currentTrack.PerformSeek(PlayerTimePosition);

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

                PlayerTimePosition += TimeSpan.FromMilliseconds(FRAMES_TO_MS) * ((double)cnt / BUFFER_SIZE);

                if (!currentTrack.IsLiveStream && TimeRemaining < TimeSpan.FromMilliseconds(TRANSMIT_SINK_MS))
                {
                    // track should be ended
                    return LowPlayerResult.Success;
                }

                // when discord voice server changed
                // needs to be handled more propertly
                if (!Handler.Voice.WriteAsync(PlayerByteBuffer, cnt).Wait(TRANSMIT_SINK_MS * 100))
                {
                    Handler.Voice.UpdateVoiceConnection();
                    Handler.Voice.UpdateSink();
                    return LowPlayerResult.RestartWrite;
                }

                Status = PlayerStatus.Playing;
            }
        }

        private ReadBytesResult PerformRead(byte[] buff, out int bytesReadCount)
        {
            bytesReadCount = 0;
            CancellationTokenSource cts = new();
            Task<int>? read_task = FfmpegInstance.ReadAsync(buff, 0, buff.Length, cts.Token);
            if (read_task == null)
            {
                return ReadBytesResult.TaskNull;
            }
            if (!read_task.Wait(TRANSMIT_SINK_MS))
            {
                cts.Cancel();
                bytesReadCount = 0;
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
