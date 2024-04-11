using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void ShuffleQueue(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            if (tracksQueue.Count != 0)
            {
                lock (queueLock)
                {
                    List<ITrackInfo> collection = [];
                    while (tracksQueue.Count != 0)
                    {
                        ITrackInfo? track = tracksQueue.Dequeue();
                        if (track != null)
                        {
                            collection.Add(track);
                        }
                    }
                    collection = collection.Shuffle().ToList();
                    tracksQueue.EnqueueRange(collection);
                }

                if (nomute)
                {
                    Handler.Message.Send(new ShuffleException("Queue shuffled").WithSuccess());
                }
            }
            else
            {
                if (nomute)
                {
                    Handler.Message.Send(new ShuffleException("Nothing to shuffle"));
                }
                return;
            }
        }
    }
}
