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
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (tracks_queue.Count != 0)
            {
                lock (tracks_queue)
                {
                    List<ITrackInfo> collection = [];
                    while (tracks_queue.Count != 0)
                    {
                        ITrackInfo? track = tracks_queue.Dequeue();
                        if (track != null)
                        {
                            collection.Add(track);
                        }
                    }
                    collection = collection.Shuffle().ToList();
                    tracks_queue.EnqueueRange(collection);
                }

                if (!mute)
                {
                    Handler.Message.Send(new ShuffleException("Queue shuffled"), true);
                }
            }
            else
            {
                if (!mute)
                {
                    throw new ShuffleException("Nothing to shuffle");
                }
                return;
            }
        }
    }
}
