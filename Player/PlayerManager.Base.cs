﻿using DicordNET.ApiClasses;
using DicordNET.Bot;
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
        internal static TimeSpan Seek;

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
                    Task.Yield().GetAwaiter().GetResult();
                }
                while (IsPlaying)
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Task.Delay(1).Wait();
                    Task.Yield().GetAwaiter().GetResult();
                }

                if (BotWrapper.VoiceConnection == null)
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }
                    Task.Delay(1).Wait();
                    Task.Yield().GetAwaiter().GetResult();
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
            if (track is null
                || BotWrapper.VoiceConnection is null)
            {
                return;
            }

            bool play_message = true;
            bool already_restartd = false;

            byte[] buff = new byte[BUFFER_SIZE];

            try
            {
                BotWrapper.TransmitSink = BotWrapper.VoiceConnection.GetTransmitSink(TRANSMIT_SINK_MS);
            }
            catch { }

        restart:

            Seek = TimeSpan.Zero;

            try
            {
                track.ObtainAudioURL();
            }
            catch
            {
                return;
            }

        seek:

            track.PerformSeek(Seek);

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

            bool exit = ffmpeg.WaitForExit(track.IsLiveStream ? 2000 : 1000);

            if (ffmpeg.HasExited || exit)
            {
                if (already_restartd)
                {
                    throw new Exception("Cannot reauth");
                }
                Console.WriteLine($"{track.TrackType} : Session expired");
                track.Reload();
                already_restartd = true;
                goto restart;
            }

            BotWrapper.SendSpeaking(true); // send a speaking indicator

            while (true)
            {
                while (IsPaused && IsPlaying && !SeekRequested)
                {
                    Task.Yield().GetAwaiter().GetResult();
                    Task.Delay(1).Wait();
                    Task.Yield().GetAwaiter().GetResult();
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
                    CancellationToken token = cts.Token;
                    Task<int> read_task = ffmpeg.StandardOutput.BaseStream.ReadAsync(buff, 0, buff.Length, token);
                    if (!read_task.Wait(100))
                    {
                        cts.Cancel();
                        bytesCount = 0;
                        if (!read_task.IsCanceled || !read_task.IsCompleted)
                        {
                            read_task.Wait();
                        }
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

                if (Seek >= track.Duration)
                {
                    // track shold be ended
                    Console.WriteLine("Stop ffmpeg");
                    break;
                }

                if (BotWrapper.TransmitSink == null)
                {
                    throw new ArgumentNullException(nameof(BotWrapper.TransmitSink), "Transmit sink not configured");
                }

                if (!BotWrapper.TransmitSink.WriteAsync(buff).Wait(TRANSMIT_SINK_MS * 100))
                {
                    BotWrapper.TransmitSink = BotWrapper.VoiceConnection.GetTransmitSink(TRANSMIT_SINK_MS);
                    continue;
                }
            }

            BotWrapper.SendSpeaking(false); // we're not speaking anymore

            IsPlaying = false;

            TrackManager.DisposeFFMPEG(ffmpeg);
        }
    }
}
