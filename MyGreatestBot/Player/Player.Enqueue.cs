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

            Queuing = true;

            lock (tracks_queue)
            {
                if (source.HasFlag(CommandActionSource.PlayerNoBlacklist))
                {
                    foreach (ITrackInfo track in tracks)
                    {
                        track.BypassCheck = true;
                    }
                }

                if (source.HasFlag(CommandActionSource.PlayerRadio))
                {
                    foreach (ITrackInfo track in tracks)
                    {
                        track.Radio = true;
                    }
                }

                if (source.HasFlag(CommandActionSource.PlayerShuffle))
                {
                    tracks = tracks.Shuffle();
                }

                if (source.HasFlag(CommandActionSource.PlayerToHead))
                {
                    tracks_queue.EnqueueRangeToHead(tracks);
                    if (source.HasFlag(CommandActionSource.PlayerSkipCurrent))
                    {
                        IsPlaying = false;
                        WaitForFinish();
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
                        $"Total: {totalCount}").WithSuccess());
                }
            }

            Queuing = false;
        }
    }
}
