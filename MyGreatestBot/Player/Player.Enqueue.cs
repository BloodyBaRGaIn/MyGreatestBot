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
        internal void Enqueue(ref IEnumerable<BaseTrackInfo> tracks, CommandActionSource source)
        {
            int totalCount;

            lock (queueLock)
            {
                if (source.HasFlag(CommandActionSource.PlayerNoBlacklist))
                {
                    foreach (BaseTrackInfo track in tracks)
                    {
                        track.BypassCheck = true;
                    }
                }

                if (source.HasFlag(CommandActionSource.PlayerRadio))
                {
                    foreach (BaseTrackInfo track in tracks)
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
                    tracksQueue.EnqueueRangeToHead(tracks);
                    if (source.HasFlag(CommandActionSource.PlayerSkipCurrent))
                    {
                        IsPlaying = false;
                        WaitForFinish();
                    }
                }
                else
                {
                    tracksQueue.EnqueueRange(tracks);
                }

                totalCount = tracksQueue.Count;

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(new PlayerException(
                        string.Join(Environment.NewLine,
                            $"Added: {tracks.Count()}",
                            $"Total: {totalCount}")).WithSuccess());
                }
            }
        }
    }
}
