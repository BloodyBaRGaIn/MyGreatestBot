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
        internal void Enqueue(IEnumerable<ITrackInfo> tracks, CommandActionSource source)
        {
            int totalCount;

            lock (tracks_queue)
            {
                if (source.HasFlag(CommandActionSource.PlayerShuffle))
                {
                    tracks = tracks.Shuffle();
                }

                if (source.HasFlag(CommandActionSource.PlayerToHead))
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

                    Skip(0, source | CommandActionSource.Mute);
                }
                else
                {
                    foreach (ITrackInfo track in tracks)
                    {
                        tracks_queue.Enqueue(track);
                    }
                }

                totalCount = tracks_queue.Count;

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(new PlayerException(
                        $"Added: {tracks.Count()}\n" +
                        $"Total: {totalCount}"), true);
                }
            }
        }
    }
}
