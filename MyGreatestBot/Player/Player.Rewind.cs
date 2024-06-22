using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
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

                        builder = new RewindException(currentTrack.GetMessage("Playing"))
                            .WithSuccess().GetDiscordEmbed();
                        builder.Thumbnail = currentTrack.GetThumbnail();
                    }
                    else
                    {
                        builder = new RewindException("Cannot seek").GetDiscordEmbed();
                    }
                }
                else
                {
                    builder = new RewindException("Nothing to seek").GetDiscordEmbed();
                }

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(builder);
                }
            }
        }
    }
}
