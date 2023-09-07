using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
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
                    tracks_queue.EnqueueRangeToHead(tracks);

                    if (source.HasFlag(CommandActionSource.PlayerSkipCurrent))
                    {
                        Skip(0, source | CommandActionSource.Mute);
                    }
                }
                else
                {
                    tracks_queue.EnqueueRange(tracks);
                }

                totalCount = tracks_queue.Count;

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(new PlayerException(
                        $"Added: {tracks.Count()}{Environment.NewLine}" +
                        $"Total: {totalCount}"), true);
                }
            }
        }
    }
}
