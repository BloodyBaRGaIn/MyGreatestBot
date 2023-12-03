using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;
        private const int FRAMES_TO_MS = TRANSMIT_SINK_MS * 2;
        private static readonly TimeSpan MaxTrackDuration = TimeSpan.FromHours(101);
        private static readonly TimeSpan MinTrackDuration = TimeSpan.FromSeconds(3);

        internal static int TransmitSinkDelay => TRANSMIT_SINK_MS;

        private volatile ITrackInfo? currentTrack;

        private volatile bool IsPlaying;
        private volatile bool IsPaused;
        private volatile bool SeekRequested;
        private volatile bool StopRequested;

        private TimeSpan Seek;

        private TimeSpan TimeRemaining => (currentTrack == null ? Seek : currentTrack.Duration) - Seek;

        private readonly Queue<ITrackInfo?> tracks_queue = new();
        private readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private readonly CancellationToken MainPlayerCancellationToken;
        private readonly Task MainPlayerTask;
        private readonly FFMPEG ffmpeg = new();

        private readonly ConnectionHandler Handler;

        private static readonly Semaphore _sqlSemaphore = new(1, 1);

        private static readonly byte[] PlayerByteBuffer = new byte[BUFFER_SIZE];

        internal Player(ConnectionHandler handler)
        {
            Handler = handler;
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            try
            {
                MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
                if (!FFMPEG.CheckForExecutableExists())
                {
                    throw new FileNotFoundException($"ffmpeg executable file not found{Environment.NewLine}" +
                        "https://github.com/BtbN/FFmpeg-Builds/releases/download/latest/ffmpeg-master-latest-win64-gpl.zip");
                }
            }
            catch (Exception ex)
            {
                throw new PlayerException("Cannot start player", ex);
            }
        }

        private static void Wait(int ms = 1)
        {
            Task.Yield().GetAwaiter().GetResult();
            Task.Delay(ms).Wait();
            Task.Yield().GetAwaiter().GetResult();
        }

        private async void PlayerTaskFunction()
        {
            try
            {
                Thread.CurrentThread.Name = nameof(PlayerTaskFunction);
            }
            catch { }

            try
            {
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }
            catch
            {
                try
                {
                    Thread.CurrentThread.Priority = ThreadPriority.AboveNormal;
                }
                catch { }
            }

            while (true)
            {
                if (MainPlayerCancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (!tracks_queue.Any())
                {
                    Wait();
                    continue;
                }

                try
                {
                    Task.Run(Handler.Voice.WaitForConnectionAsync, MainPlayerCancellationToken).Wait();

                    if (currentTrack == null)
                    {
                        Dequeue();
                    }

                    if (currentTrack != null)
                    {
                        PlayBody();
                    }

                    if (!tracks_queue.Any() && !StopRequested)
                    {
                        Handler.Message.Send(new PlayerException("No more tracks"));
                    }

                    StopRequested = false;
                }
                catch (TaskCanceledException)
                {
                    return;
                }
                catch (TypeInitializationException ex)
                {
                    Stop(CommandActionSource.Mute);
                    await Handler.LogError.SendAsync(ex.GetExtendedMessage());
                    Environment.Exit(1);
                    return;
                }
                catch (Exception ex)
                {
                    IsPlaying = false;
                    await Handler.LogError.SendAsync(ex.GetExtendedMessage());
                }
            }
        }

        private bool TryObtainAudio()
        {
            if (currentTrack is null)
            {
                return false;
            }

            Seek = currentTrack.Seek;

            try
            {
                currentTrack.ObtainAudioURL();
            }
            catch (Exception ex)
            {
                Handler.Message.Send(ex.GetExtendedMessage());
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

            bool already_restarted = false;
            bool obtain_audio = true;

            try
            {
                Handler.Voice.UpdateSink();
            }
            catch { }

            Handler.Log.Send(currentTrack.GetShortMessage("Playing: "));

            IsPlaying = true;

            while (true)
            {
                Wait(100);

                if (obtain_audio)
                {
                    if (TryObtainAudio())
                    {
                        if (!IAccessible.IsUrlSuccess(currentTrack.AudioURL, false))
                        {
                            Handler.LogError.Send("Audio URL is not available");
                            currentTrack.Reload();
                            already_restarted = true;
                            continue;
                        }
                    }
                    else
                    {
                        Handler.LogError.Send("Cannot obtain audio URL");
                        break;
                    }
                }

                currentTrack.PerformSeek(Seek);

                ffmpeg.Start(currentTrack);

                Handler.Log.Send("Start ffmpeg");

                if (!ffmpeg.TryLoad(3000))
                {
                    Wait(100);

                    if (!currentTrack.IsLiveStream && TimeRemaining < MinTrackDuration)
                    {
                        break;
                    }
                    if (already_restarted)
                    {
                        throw new ApiException(currentTrack.TrackType);
                    }
                    currentTrack.Reload();
                    already_restarted = true;
                    obtain_audio = true;
                    continue;
                }

                Handler.Voice.SendSpeaking(true);

                LowPlayerResult low_result = LowPlayer();

                Handler.Voice.SendSpeaking(false);

                if (low_result == LowPlayerResult.Restart)
                {
                    Handler.Log.Send("Restart ffmpeg");

                    string errorMessage = ffmpeg.GetErrorMessage();

                    if (string.IsNullOrWhiteSpace(errorMessage))
                    {
                        Handler.Log.Send("Unexpected process exit");
                    }
                    else
                    {
                        Handler.Log.Send(Environment.NewLine +
                            $"Error message begin{Environment.NewLine}" +
                            $"{errorMessage}{Environment.NewLine}" +
                            "Error message end");
                    }

                    obtain_audio = false;
                    continue;
                }
                else
                {
                    break;
                }
            }

            IsPlaying = false;

            ffmpeg.Stop();

            Handler.Log.Send("Stop ffmpeg");

            currentTrack = null;
        }

        private enum LowPlayerResult : int
        {
            TrackNull = -1,
            Success = 0,
            Restart = 1
        }

        private LowPlayerResult LowPlayer()
        {
            if (currentTrack == null)
            {
                return LowPlayerResult.TrackNull;
            }

            while (true)
            {
                while (IsPaused && IsPlaying && !SeekRequested)
                {
                    Wait();
                }

                if (!IsPlaying)
                {
                    return LowPlayerResult.Success;
                }

                if (SeekRequested)
                {
                    SeekRequested = false;
                    IsPaused = false;
                    Seek = currentTrack.Seek;
                    return LowPlayerResult.Restart;
                }

                currentTrack.PerformSeek(Seek);

                if (!PerformRead(PlayerByteBuffer, out int cnt))
                {
                    if (!currentTrack.IsLiveStream && TimeRemaining < MinTrackDuration)
                    {
                        // track almost ended
                        return LowPlayerResult.Success;
                    }

                    // restart ffmpeg
                    return LowPlayerResult.Restart;
                }

                Seek += TimeSpan.FromMilliseconds(FRAMES_TO_MS) * ((double)cnt / BUFFER_SIZE);

                if (!currentTrack.IsLiveStream && TimeRemaining < TimeSpan.FromMilliseconds(TRANSMIT_SINK_MS))
                {
                    // track should be ended
                    return LowPlayerResult.Success;
                }

                if (!Handler.Voice.WriteAsync(PlayerByteBuffer).Wait(TRANSMIT_SINK_MS * 100))
                {
                    Handler.Voice.WaitForConnectionAsync().Wait();
                    Handler.Voice.UpdateSink();
                }
            }
        }

        private bool PerformRead(byte[] buff, out int origin_cnt)
        {
            int bytesCount;
            origin_cnt = 0;
            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;
            Task<int>? read_task = ffmpeg.StandardOutput?.BaseStream?.ReadAsync(buff, 0, buff.Length, token);
            if (read_task == null)
            {
                return false;
            }
            if (!read_task.Wait(TRANSMIT_SINK_MS * 100))
            {
                cts.Cancel();
                bytesCount = 0;
            }
            else
            {
                bytesCount = read_task.Result;
            }
            if (bytesCount == 0)
            {
                return false;
            }

            origin_cnt = bytesCount;
            if (bytesCount < buff.Length)
            {
                Task.Delay(TRANSMIT_SINK_MS).Wait();
                while (bytesCount < buff.Length)
                {
                    buff[bytesCount++] = 0;
                }
            }

            return true;
        }
    }
}
