using DicordNET.Bot;
using DicordNET.Commands;
using DicordNET.TrackClasses;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Enqueue(IEnumerable<ITrackInfo> tracks, CommandActionSource source = CommandActionSource.None)
        {
            int count;

            lock (tracks_queue)
            {
                if ((source & CommandActionSource.External) == 0)
                {
                    foreach (var track in tracks)
                    {
                        tracks_queue.Enqueue(track);
                    }
                }
                else
                {
                    List<ITrackInfo> collection = new();
                    collection.AddRange(tracks);
                    while (tracks_queue.Any())
                    {
                        collection.Add(tracks_queue.Dequeue());
                    }
                    while (collection.Any())
                    {
                        tracks_queue.Enqueue(collection[0]);
                        collection.RemoveAt(0);
                    }
                }

                count = tracks_queue.Count;
            }

            if ((source & CommandActionSource.Mute) == 0)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Play",
                    Description = $"Added: {tracks.Count()}\n" +
                                  $"Total: {count}"
                });
            }
        }
    }
}
