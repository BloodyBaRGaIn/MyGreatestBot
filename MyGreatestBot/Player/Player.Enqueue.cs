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
                    ReturnCurrentTrackToQueue(source | CommandActionSource.Mute);
                    tracks_queue.EnqueueRangeToHead(tracks);
                }
                else
                {
                    tracks_queue.EnqueueRange(tracks);
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
