using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
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
                    while (collection.Any())
                    {
                        tracks_queue.Enqueue(collection[0]);
                        collection.RemoveAt(0);
                    }
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
