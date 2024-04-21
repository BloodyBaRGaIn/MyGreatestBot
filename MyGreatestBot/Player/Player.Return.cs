using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void ReturnCurrentTrackToQueue(CommandActionSource source)
        {
            DiscordEmbedBuilder builder;

            if (currentTrack == null)
            {
                builder = new ReturnException("Nothing to return").GetDiscordEmbed();
            }

            lock (queueLock)
            {
                lock (trackLock)
                {
                    if (currentTrack == null)
                    {
                        builder = new ReturnException("Nothing to return").GetDiscordEmbed();
                    }
                    else
                    {
                        if (source.HasFlag(CommandActionSource.PlayerToHead))
                        {
                            tracksQueue.EnqueueToHead(currentTrack);
                        }
                        else
                        {
                            currentTrack.PerformSeek(TimeSpan.Zero);
                            tracksQueue.Enqueue(currentTrack);
                        }
                        builder = new ReturnException("Returned to queue").WithSuccess().GetDiscordEmbed();
                    }
                }

                IsPlaying = false;

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(builder);
                }
            }
        }
    }
}
