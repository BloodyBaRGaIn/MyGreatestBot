using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void ReturnCurrentTrackToQueue(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (currentTrack == null)
            {
                if (!mute)
                {
                    throw new ReturnException("Nothing to return");
                }
                return;
            }

            lock (tracks_queue)
            {
                lock (currentTrack)
                {
                    if (source.HasFlag(CommandActionSource.PlayerToHead))
                    {
                        tracks_queue.EnqueueToHead(currentTrack);
                    }
                    else
                    {
                        currentTrack.PerformSeek(TimeSpan.Zero);
                        tracks_queue.Enqueue(currentTrack);
                    }
                }

                IsPlaying = false;

                if (!mute)
                {
                    Handler.Message.Send(new ReturnException("Returned to queue"), true);
                }
            }
        }
    }
}
