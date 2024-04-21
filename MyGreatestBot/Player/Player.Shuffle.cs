using DSharpPlus.Entities;
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
            DiscordEmbedBuilder builder;

            if (tracksQueue.Count == 0)
            {
                builder = new ShuffleException("Nothing to shuffle").GetDiscordEmbed();
            }
            else
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

                builder = new ShuffleException("Queue shuffled").WithSuccess().GetDiscordEmbed();
            }

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                Handler.Message.Send(builder);
            }
        }
    }
}
