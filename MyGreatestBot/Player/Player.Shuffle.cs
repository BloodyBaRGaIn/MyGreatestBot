using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
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
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            if (tracksQueue.Count == 0)
            {
                messageHandler?.Send(new ShuffleException("Nothing to shuffle"));
                return;
            }

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

            messageHandler?.Send(new ShuffleException("Queue shuffled").WithSuccess());
        }
    }
}
