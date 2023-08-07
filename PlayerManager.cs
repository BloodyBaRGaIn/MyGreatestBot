using DicordNET.TrackClasses;
using DSharpPlus.Entities;
using System.Diagnostics;

namespace DicordNET
{
    internal static class PlayerManager
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

        internal static void EnqueueTracks(IEnumerable<ITrackInfo> tracks, ActionSource source = ActionSource.None)
        {
            int count;

            lock (tracks_queue)
            {
                if ((source & ActionSource.External) == 0)
                {
                    foreach (var track in tracks)
                    {
                        tracks_queue.Enqueue(track);
                    }
                }
                else
                {
                    List<ITrackInfo> collection = new();
                    collection.AddRange(tracks);
                    while (tracks_queue.Any())
                    {
                        collection.Add(tracks_queue.Dequeue());
                    }
                    while (collection.Any())
                    {
                        tracks_queue.Enqueue(collection[0]);
                        collection.RemoveAt(0);
                    }
                }

                count = tracks_queue.Count;
            }

            if ((source & ActionSource.Mute) == 0)
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Play",
                    Description = $"Added: {tracks.Count()}\n" +
                                  $"Total: {count}"
                });
            }
        }

        internal static void Pause(ActionSource source = ActionSource.None)
        {
            if ((source & ActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Paused"
                    });
                }
                else
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Nothing to pause"
                    });
                }
            }
            IsPaused = true;
        }

        internal static void Resume(ActionSource source = ActionSource.None)
        {
            if ((source & ActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Resumed"
                    });
                }
                else
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Nothing to resume"
                    });
                }
            }
            IsPaused = false;
        }

        internal static void Clear(ActionSource source = ActionSource.None)
        {
            lock (tracks_queue)
            {
                tracks_queue.Clear();
                IsPlaying = false;
                currentTrack = null;
            }
        }

        internal static void Stop(ActionSource source = ActionSource.None)
        {
            if (IsPlaying || tracks_queue.Any())
            {
                if ((source & ActionSource.External) != 0)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Kicked from voice channel"
                    });
                    return;
                }

                Clear(source);
                if ((source & ActionSource.Mute) == 0)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Stopped"
                    });
                }
            }
            else
            {
                if ((source & (ActionSource.Mute | ActionSource.External)) == 0)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to stop"
                    });
                }
            }
        }

        internal static void Terminate(ActionSource source = ActionSource.None)
        {
            MainPlayerCancellationTokenSource.Cancel();
            Clear(source);
        }

        internal static void Skip(ActionSource source = ActionSource.None)
        {
            if ((source & ActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Skipped"
                    });
                }
                else
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to skip"
                    });
                }
            }
            IsPlaying = false;
        }

        internal static void GetCurrentTrackInfo()
        {
            if (IsPlaying && currentTrack != null)
            {
                lock (currentTrack)
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Purple,
                        Title = "Track",
                        Description = currentTrack.GetMessage(),
                        Thumbnail = currentTrack.GetThumbnail()
                    });
                }
            }
            else
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Track",
                    Description = "No tracks playing"
                });
            }
        }

        internal static void GetQueueLength()
        {
            if (tracks_queue.Any())
            {
                int count;
                int live_streams_count;
                TimeSpan total_duration = TimeSpan.Zero;

                lock (tracks_queue)
                {
                    count = tracks_queue.Count;
                    live_streams_count = tracks_queue.Count(t => t.IsLiveStream || t.Duration == TimeSpan.Zero);
                    total_duration = tracks_queue.Aggregate(TimeSpan.Zero, (sum, next) => sum + next.Duration);
                }

                string description = $"Enqueued tracks count: {count}\n";

                if (live_streams_count != 0)
                {
                    description += $"Enqueued live streams: {live_streams_count}\n";
                }

                description += $"Total duration: {total_duration:dd\\.hh\\:mm\\:ss}";

                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Count",
                    Description = description
                });
            }
            else
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Count",
                    Description = "Tracks queue is empty"
                });
            }
        }

        internal static void ShuffleQueue()
        {
            if (tracks_queue.Any())
            {
                lock (tracks_queue)
                {
                    Random rnd = new();
                    List<ITrackInfo> collection = new();
                    while (tracks_queue.Any())
                    {
                        collection.Add(tracks_queue.Dequeue());
                    }
                    collection = collection.OrderBy(x => rnd.Next()).ToList();
                    while (collection.Any())
                    {
                        tracks_queue.Enqueue(collection[0]);
                        collection.RemoveAt(0);
                    }
                }

                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Orange,
                    Title = "Shuffle",
                    Description = "Queue shuffled"
                });
            }
            else
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Shuffle",
                    Description = "Nothing to shuffle"
                });
            }
        }

        internal static void ReturnCurrentTrackToQueue()
        {
            if (currentTrack == null)
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Return",
                    Description = "Nothing to return"
                });
            }
            else
            {
                lock (tracks_queue)
                {
                    lock (currentTrack)
                    {
                        tracks_queue.Enqueue(currentTrack);
                    }
                }

                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Return",
                    Description = "Returned to queue"
                });
            }

            IsPlaying = false;
        }

        internal static void RequestSeek(TimeSpan span)
        {
            if (!IsPlaying || currentTrack == null)
            {
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Seek",
                    Description = "Nothing to seek"
                });
            }
            else
            {
                if (currentTrack != null
                    && !currentTrack.IsLiveStream
                    && currentTrack.Duration != TimeSpan.Zero
                    && currentTrack.Duration > span)
                {
                    lock (currentTrack)
                    {
                        Seek = span;
                        currentTrack.Seek = Seek;

                        StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Purple,
                            Title = "Seek",
                            Description = currentTrack.GetMessage(),
                            Thumbnail = currentTrack.GetThumbnail()
                        });
                    }

                    IsPaused = true;

                    Task.Yield().GetAwaiter().GetResult();

                    SeekRequested = true;
                }
                else
                {
                    StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Seek",
                        Description = "Cannot seek"
                    });
                }
            }
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

                if (StaticBotInstanceContainer.VoiceConnection == null)
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
                        StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
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
                || StaticBotInstanceContainer.VoiceConnection == null
                || track == null)
            {
                return;
            }

            bool play_message = true;

            byte[] buff = new byte[BUFFER_SIZE];

            try
            {
                StaticBotInstanceContainer.TransmitSink = StaticBotInstanceContainer.VoiceConnection.GetTransmitSink(TRANSMIT_SINK_MS);
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
                StaticBotInstanceContainer.SendMessage(new DiscordEmbedBuilder()
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
                StaticBotInstanceContainer.SendMessage("Session expired");
                track.Reload();
                goto restart;
            }

            StaticBotInstanceContainer.SendSpeaking(true); // send a speaking indicator

            while (true)
            {
                while (IsPaused && IsPlaying && !SeekRequested)
                {
                    Task.Delay(1).Wait();
                }

                if (!IsPlaying)
                {
                    goto finish;
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

                if (StaticBotInstanceContainer.TransmitSink == null)
                {
                    throw new ArgumentNullException(nameof(StaticBotInstanceContainer.TransmitSink));
                }

                if (!StaticBotInstanceContainer.TransmitSink.WriteAsync(buff).Wait(TRANSMIT_SINK_MS * 100))
                {
                    //goto finish;
                }
            }

            StaticBotInstanceContainer.SendSpeaking(false); // we're not speaking anymore

        finish:

            IsPlaying = false;

            TrackManager.DisposeFFMPEG(ffmpeg);
        }
    }
}
