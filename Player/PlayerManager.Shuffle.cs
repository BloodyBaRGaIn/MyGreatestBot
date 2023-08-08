using DicordNET.Bot;
using DicordNET.TrackClasses;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void ShuffleQueue()
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

                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Orange,
                    Title = "Shuffle",
                    Description = "Queue shuffled"
                });
            }
            else
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Shuffle",
                    Description = "Nothing to shuffle"
                });
            }
        }
    }
}
