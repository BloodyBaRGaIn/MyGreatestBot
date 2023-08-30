using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        private const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;
        private const int FRAMES_TO_MS = TRANSMIT_SINK_MS * 2;

        private static readonly TimeSpan MaxTrackDuration = TimeSpan.FromHours(20);

        internal int TransmitSinkDelay { get; private set; } = TRANSMIT_SINK_MS;

        private volatile ITrackInfo? currentTrack;

        private volatile bool IsPlaying;
        private volatile bool IsPaused;
        private volatile bool SeekRequested;
        private volatile bool StopRequested;

        private TimeSpan Seek;

        private readonly Queue<ITrackInfo> tracks_queue = new();
        private readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private readonly CancellationToken MainPlayerCancellationToken;
        private readonly Task MainPlayerTask;
        private readonly FFMPEG ffmpeg = new();

        private readonly ConnectionHandler Handler;

        internal Player(ConnectionHandler handler)
        {
            Handler = handler;
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
        }

        private static void Wait()
        {
            Task.Delay(1).Wait();
            Task.Yield().GetAwaiter().GetResult();
        }

        private async void PlayerTaskFunction()
        {
            try
            {
                Thread.CurrentThread.Name = nameof(PlayerTaskFunction);
                Thread.CurrentThread.Priority = ThreadPriority.Highest;
            }
            catch { }

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

                if (Handler.VoiceConnection == null)
                {
                    Handler.VoiceConnection = Handler.GetVoiceConnection();
                    Wait();
                    continue;
                }

                try
                {
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
                        await Handler.SendMessageAsync(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "Play",
                            Description = "No more tracks"
                        });
                    }

                    StopRequested = false;
                }
                catch (Exception ex) when (ex is TypeInitializationException)
                {
                    Clear(Commands.CommandActionSource.Mute);
                    await Handler.LogErrorAsync(ex.GetExtendedMessage());
                    Environment.Exit(1);
                    return;
                }
                catch (Exception ex) when (ex is TaskCanceledException)
                {
                    return;
                }
                catch (Exception ex)
                {
                    IsPlaying = false;
                    await Handler.LogErrorAsync(ex.GetExtendedMessage());
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
                Handler.SendMessage(ex.GetExtendedMessage());
                return false;
            }

            return true;
        }

        private void PlayBody()
        {
            if (currentTrack is null
                || Handler.VoiceConnection is null)
            {
                return;
            }

            bool already_restartd = false;
            bool obtain_audio = true;

            try
            {
                Handler.UpdateSink();
            }
            catch { }

            Handler.Log(currentTrack.GetShortMessage());

            IsPlaying = true;

            while (true)
            {
                if (obtain_audio && !TryObtainAudio())
                {
                    break;
                }

                currentTrack.PerformSeek(Seek);

                ffmpeg.Start(currentTrack);

                Handler.Log("Start ffmpeg");

                if (!ffmpeg.TryLoad(currentTrack.IsLiveStream ? 2000 : (int)(1000 * (currentTrack.Duration.TotalHours + 1))))
                {
                    if (already_restartd)
                    {
                        throw new GenericApiException(currentTrack.TrackType, "Cannot reauth");
                    }
                    currentTrack.Reload();
                    already_restartd = true;
                    obtain_audio = true;
                    continue;
                }

                Handler.SendSpeaking(true);

                LowPlayerResult low_result = LowPlayer();

                Handler.SendSpeaking(false);

                if (low_result == LowPlayerResult.Restart)
                {
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

            Handler.Log("Stop ffmpeg");

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

            byte[] buff = new byte[BUFFER_SIZE];

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

                if (!PerformRead(buff))
                {
                    if (!currentTrack.IsLiveStream && currentTrack.Duration - Seek < TimeSpan.FromSeconds(5))
                    {
                        // track almost ended
                        return LowPlayerResult.Success;
                    }

                    // restart ffmpeg
                    Handler.Log("Restart ffmpeg");
                    return LowPlayerResult.Restart;
                }

                Seek += TimeSpan.FromMilliseconds(FRAMES_TO_MS);

                if (!currentTrack.IsLiveStream && currentTrack.Duration - Seek <= TimeSpan.FromSeconds(1))
                {
                    // track should be ended
                    return LowPlayerResult.Success;
                }

                while (Handler.TransmitSink == null)
                {
                    Handler.UpdateSink();
                    Wait();
                }

                if (!Handler.TransmitSink.WriteAsync(buff).Wait(TransmitSinkDelay * 100))
                {
                    Handler.WaitForConnectionAsync().Wait();
                    Handler.UpdateSink();
                }
            }
        }

        private bool PerformRead(byte[] buff)
        {
            int bytesCount;
            CancellationTokenSource cts = new();
            CancellationToken token = cts.Token;
            Task<int>? read_task = ffmpeg.StandardOutput?.BaseStream?.ReadAsync(buff, 0, buff.Length, token);
            if (read_task == null)
            {
                return false;
            }
            if (!read_task.Wait(100))
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

            if (bytesCount < buff.Length)
            {
                Task.Delay(100).Wait();
                while (bytesCount < buff.Length)
                {
                    buff[bytesCount++] = 0;
                }
            }
            return true;
        }
    }
}
