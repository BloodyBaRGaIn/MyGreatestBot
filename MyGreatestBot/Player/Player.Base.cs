using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Bot;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        private const string IgnoredPath = "IgnoredTracks.txt";

        private readonly Queue<ITrackInfo> tracks_queue = new();

        private volatile ITrackInfo? currentTrack;

        private volatile bool IsPlaying;
        private volatile bool IsPaused;
        private volatile bool SeekRequested;
        internal TimeSpan Seek;

        internal const int TRANSMIT_SINK_MS = 10;
        private const int BUFFER_SIZE = 1920 * TRANSMIT_SINK_MS / 5;
        private const int FRAMES_TO_MS = TRANSMIT_SINK_MS * 2;

        private readonly CancellationTokenSource MainPlayerCancellationTokenSource = new();
        private readonly CancellationToken MainPlayerCancellationToken;
        private readonly Task MainPlayerTask;

        private readonly ConnectionHandler Handler;

        internal Player(ConnectionHandler handler)
        {
            Handler = handler;
            MainPlayerCancellationToken = MainPlayerCancellationTokenSource.Token;
            MainPlayerTask = Task.Factory.StartNew(PlayerTaskFunction, MainPlayerCancellationToken);
        }

        private void PlayerTaskFunction()
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

                if (Handler.VoiceConnection == null)
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
                    Dequeue();

                    if (currentTrack is not null)
                    {
                        PlayBody(currentTrack);
                    }

                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    if (!tracks_queue.Any())
                    {
                        Handler.SendMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Red,
                            Title = "Play",
                            Description = "No more tracks"
                        });
                    }
                }
                catch (Exception ex) when (ex is TypeInitializationException)
                {
                    Clear();
                    Handler.LogError(ex.GetExtendedMessage());
                    Environment.Exit(1);
                    return;
                }
                catch (Exception ex)
                {
                    if (MainPlayerCancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    IsPlaying = false;

                    Handler.LogError(ex.GetExtendedMessage());

                    continue;
                }
            }
        }

        private void PlayBody(ITrackInfo track)
        {
            if (track is null
                || Handler.VoiceConnection is null)
            {
                return;
            }

            bool play_message = true;
            bool already_restartd = false;

            byte[] buff = new byte[BUFFER_SIZE];

            try
            {
                Handler.UpdateSink();
            }
            catch { }

        restart:

            Seek = TimeSpan.Zero;

            try
            {
                track.ObtainAudioURL();
            }
            catch (Exception ex)
            {
                Handler.SendMessage(ex.GetExtendedMessage());
                return;
            }

        seek:

            track.PerformSeek(Seek);

            IsPlaying = true;

            if (play_message)
            {
                play_message = false;
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Play",
                    Description = track.GetMessage(),
                    Thumbnail = track.GetThumbnail()
                });
            }

            Process ffmpeg = TrackManager.StartFFMPEG(track);

            bool exit = ffmpeg.WaitForExit(track.IsLiveStream ? 2000 : (int)(1000 * (track.Duration.TotalHours + 1)));

            if (ffmpeg.HasExited || exit)
            {
                if (already_restartd)
                {
                    throw new Exception("Cannot reauth");
                }
                Handler.Log($"{track.TrackType} : Session expired");
                track.Reload();
                already_restartd = true;
                goto restart;
            }

            Handler.SendSpeaking(true); // send a speaking indicator

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
                    Seek = track.Seek;
                    goto seek;
                }

                track.PerformSeek(Seek);

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
                        Handler.Log("Restart ffmpeg");
                    }
                    else
                    {
                        if (track.Duration - Seek >= TimeSpan.FromSeconds(5))
                        {
                            //StaticBotInstanceContainer.SendMessage($"Restarting at {span}");
                            Handler.Log("Restart ffmpeg");
                        }
                        else
                        {
                            // track almost ended
                            Handler.Log("Stop ffmpeg");
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
                    if (Seek >= track.Duration)
                    {
                        // track shold be ended
                        Handler.Log("Stop ffmpeg");
                        break;
                    }
                }

                if (Handler.TransmitSink == null)
                {
                    throw new ArgumentNullException(nameof(Handler.TransmitSink), "Transmit sink not configured");
                }

                if (!Handler.TransmitSink.WriteAsync(buff).Wait(TRANSMIT_SINK_MS * 100))
                {
                    if (Handler.VoiceConnection == null)
                    {
                        break;
                    }

                    Handler.UpdateSink();
                    continue;
                }
            }

            Handler.SendSpeaking(false); // we're not speaking anymore

            IsPlaying = false;

            TrackManager.DisposeFFMPEG(ffmpeg);
        }
    }
}
