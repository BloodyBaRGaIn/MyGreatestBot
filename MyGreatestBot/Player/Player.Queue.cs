using MyGreatestBot.Commands.Exceptions;
using System;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void GetQueueLength()
        {
            if (tracksQueue.Count != 0)
            {
                int count;
                int live_streams_count;
                TimeSpan total_duration = TimeSpan.Zero;

                lock (queueLock)
                {
                    count = tracksQueue.Count;
                    live_streams_count = tracksQueue.Count(t => t != null && t.IsLiveStream);
                    total_duration = tracksQueue.Aggregate(TimeSpan.Zero, (sum, next) => sum + (next?.Duration ?? TimeSpan.Zero));
                }

                string description = $"Enqueued tracks count: {count}{Environment.NewLine}";

                if (live_streams_count != 0)
                {
                    description += $"Enqueued live streams: {live_streams_count}{Environment.NewLine}";
                }

                if (currentTrack != null && currentTrack.Radio)
                {
                    description += $"Player is on radio mode{Environment.NewLine}";
                }

                description += $"Total duration: {total_duration:dd\\.hh\\:mm\\:ss}";

                Handler.Message.Send(new QueueLengthException(description).WithSuccess());
            }
            else
            {
                throw new QueueLengthException("Tracks queue is empty");
            }
        }
    }
}
