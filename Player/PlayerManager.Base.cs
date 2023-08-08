using DicordNET.Bot;
using DicordNET.TrackClasses;
using DSharpPlus.Entities;
using System.Diagnostics;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        private static readonly Queue<ITrackInfo> tracks_queue = new();

        private static volatile ITrackInfo? currentTrack;

        private static volatile bool IsPlaying;
        private static volatile bool IsPaused;
        private static volatile bool SeekRequested;
        private static TimeSpan Seek;

        private const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;
        private const int FRAMES_TO_MS = TRANSMIT_SINK_MS * 2;

        private static readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private static readonly CancellationToken MainPlayerCancellationToken;
        private static readonly Task MainPlayerTask;

        static PlayerManager()
        {
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
        }

        private static void PlayerTaskFunction()
        {
            Thread.CurrentThread.Name = nameof(PlayerTaskFunction);
            Thread.CurrentThread.Priority = ThreadPriority.Highest;

            while (true)
            {
                if (MainPlayerCancellationToken.IsCancellationRequested)
                {
                    return;
                }

                while (!tracks_queue.Any())
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Task.Delay(1).Wait();
                }
                while (IsPlaying)
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Task.Delay(1).Wait();
                }

                if (BotWrapper.VoiceConnection == null)
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Task.Delay(1).Wait();
                    continue;
                }

                try
                {
                    currentTrack = tracks_queue.Dequeue();
                    PlayBody(currentTrack);

                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!tracks_queue.Any())
                    {
                        BotWrapper.SendMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "Play",
                            Description = "No more tracks"
                        });
                    }
                }
                catch
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    IsPlaying = false;
                    continue;
                }
            }
        }

        private static void PlayBody(ITrackInfo track)
        {
            if (track == null
                || BotWrapper.VoiceConnection == null
                || track == null)
            {
                return;
            }

            bool play_message = true;

            byte[] buff = new byte[BUFFER_SIZE];

            try
            {
                BotWrapper.TransmitSink = BotWrapper.VoiceConnection.GetTransmitSink(TRANSMIT_SINK_MS);
            }
            catch
            {
                ;
            }

        restart:

            Seek = TimeSpan.Zero;

            track.ObtainAudioURL();

        seek:

            if (!track.IsLiveStream)
            {
                track.Seek = Seek;
            }

            IsPlaying = true;

            if (play_message)
            {
                play_message = false;
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Play",
                    Description = track.GetMessage(),
                    Thumbnail = track.GetThumbnail()
                });
            }

            Process ffmpeg = TrackManager.StartFFMPEG(track);

            if (track.IsLiveStream)
            {
                _ = ffmpeg.WaitForExit(2000);
            }
            else
            {
                _ = ffmpeg.WaitForExit(500);
            }

            if (ffmpeg.HasExited)
            {
                BotWrapper.SendMessage("Session expired");
                track.Reload();
                goto restart;
            }

            BotWrapper.SendSpeaking(true); // send a speaking indicator

            while (true)
            {
                while (IsPaused && IsPlaying && !SeekRequested)
                {
                    Task.Delay(1).Wait();
                }

                if (!IsPlaying)
                {
                    break;
                }

                if (SeekRequested)
                {
                    SeekRequested = false;
                    IsPaused = false;
                    TrackManager.DisposeFFMPEG(ffmpeg);
                    goto seek;
                }

                int retries = 0;

                int bytesCount;

                while (retries < 2)
                {
                    CancellationTokenSource cts = new();
                    Task<int> read_task = ffmpeg.StandardOutput.BaseStream.ReadAsync(buff, 0, buff.Length, cts.Token);
                    if (!read_task.Wait(100))
                    {
                        cts.Cancel();
                        bytesCount = 0;
                    }
                    else
                    {
                        bytesCount = read_task.Result;
                    }

                    if (bytesCount != 0)
                    {
                        if (bytesCount < buff.Length)
                        {
                            Task.Delay(100).Wait();
                            while (bytesCount < buff.Length)
                            {
                                buff[bytesCount++] = 0;
                            }
                        }
                        break;
                    }

                    retries++;

                    Task.Delay(10).Wait();
                }

                if (retries == 2)
                {
                    if (track.IsLiveStream)
                    {
                        //StaticBotInstanceContainer.SendMessage("Restarting");
                        Console.WriteLine("Restart ffmpeg");
                    }
                    else
                    {
                        if (track.Duration - Seek >= TimeSpan.FromSeconds(5))
                        {
                            //StaticBotInstanceContainer.SendMessage($"Restarting at {span}");
                            Console.WriteLine("Restart ffmpeg");
                        }
                        else
                        {
                            // track almost ended
                            Console.WriteLine("Stop ffmpeg");
                            break;
                        }
                    }

                    // restart ffmpeg
                    TrackManager.DisposeFFMPEG(ffmpeg);
                    goto seek;
                }

                Seek += TimeSpan.FromMilliseconds(FRAMES_TO_MS);

                if (!track.IsLiveStream)
                {
                    track.Seek = Seek;
                }

                if (BotWrapper.TransmitSink == null)
                {
                    throw new ArgumentNullException(nameof(BotWrapper.TransmitSink));
                }

                if (!BotWrapper.TransmitSink.WriteAsync(buff).Wait(TRANSMIT_SINK_MS * 100))
                {
                    break;
                }
            }

            BotWrapper.SendSpeaking(false); // we're not speaking anymore

            IsPlaying = false;

            TrackManager.DisposeFFMPEG(ffmpeg);
        }
    }
}
