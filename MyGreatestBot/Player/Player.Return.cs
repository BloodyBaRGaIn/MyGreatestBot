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
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            if (currentTrack == null)
            {
                if (nomute)
                {
                    throw new ReturnException("Nothing to return");
                }
                return;
            }

            lock (queueLock)
            {
                lock (trackLock)
                {
                    if (source.HasFlag(CommandActionSource.PlayerToHead))
                    {
                        tracksQueue.EnqueueToHead(currentTrack);
                    }
                    else
                    {
                        currentTrack.PerformSeek(TimeSpan.Zero);
                        tracksQueue.Enqueue(currentTrack);
                    }
                }

                IsPlaying = false;

                if (nomute)
                {
                    Handler.Message.Send(new ReturnException("Returned to queue").WithSuccess());
                }
            }
        }
    }
}
