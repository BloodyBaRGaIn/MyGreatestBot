using MyGreatestBot.Commands.Exceptions;
using System;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
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

                Handler.Message.Send(new QueueLengthException(description), true);
            }
            else
            {
                throw new QueueLengthException("Tracks queue is empty");
            }
        }
    }
}
