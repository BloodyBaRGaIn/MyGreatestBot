﻿using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void RequestRewind(TimeSpan span, CommandActionSource source)
        {
            lock (trackLock)
            {
                DiscordEmbedBuilder builder;

                if (IsPlaying && currentTrack != null)
                {
                    if (currentTrack.IsRewindPossible(span))
                    {
                        currentTrack.PerformRewind(span);
                        RewindRequested = true;

                        builder = new RewindCommandException(currentTrack.GetMessage("Playing"))
                            .WithSuccess().GetDiscordEmbed();
                        builder.Thumbnail = currentTrack.Thumbnail;
                    }
                    else
                    {
                        builder = new RewindCommandException("Cannot rewind").GetDiscordEmbed();
                    }
                }
                else
                {
                    builder = new RewindCommandException("Nothing to rewind").GetDiscordEmbed();
                }

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(builder);
                }
            }
        }
    }
}
