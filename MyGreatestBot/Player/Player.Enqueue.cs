using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Enqueue(IEnumerable<ITrackInfo> tracks, CommandActionSource source = CommandActionSource.None)
        {
            int count;

            lock (tracks_queue)
            {
                if (!source.HasFlag(CommandActionSource.External))
                {
                    foreach (ITrackInfo track in tracks)
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

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
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
