using MyGreatestBot.ApiClasses;
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
            if (tracks_queue.Any())
            {
                lock (tracks_queue)
                {
                    List<ITrackInfo> collection = new();
                    while (tracks_queue.Any())
                    {
                        collection.Add(tracks_queue.Dequeue());
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
