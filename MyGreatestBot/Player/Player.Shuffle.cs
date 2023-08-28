using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void ShuffleQueue()
        {
            if (tracks_queue.Any())
            {
                lock (tracks_queue)
                {
                    Random rnd = new();
                    List<ITrackInfo> collection = new();
                    while (tracks_queue.Any())
                    {
                        collection.Add(tracks_queue.Dequeue());
                    }
                    collection = collection.OrderBy(x => rnd.Next()).ToList();
                    while (collection.Any())
                    {
                        tracks_queue.Enqueue(collection[0]);
                        collection.RemoveAt(0);
                    }
                }

                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Orange,
                    Title = "Shuffle",
                    Description = "Queue shuffled"
                });
            }
            else
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Shuffle",
                    Description = "Nothing to shuffle"
                });
            }
        }
    }
}
