using DSharpPlus.Entities;
using System;
using System.Linq;

namespace DicordNET.Player
{
    internal partial class Player
    {
        internal void GetQueueLength()
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

                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Count",
                    Description = description
                });
            }
            else
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Count",
                    Description = "Tracks queue is empty"
                });
            }
        }
    }
}
