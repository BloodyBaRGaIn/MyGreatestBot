using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void RequestSeek(TimeSpan span, CommandActionSource source)
        {
            lock (trackLock)
            {
                DiscordEmbedBuilder builder;

                if (IsPlaying && currentTrack != null)
                {
                    if (currentTrack.IsSeekPossible(span))
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;

                        try
                        {
                            builder = GetPlayingMessage<SeekException>(currentTrack, "Playing");
                        }
                        catch
                        {
                            builder = new SeekException("Cannot make seek message").GetDiscordEmbed();
                        }
                    }
                    else
                    {
                        builder = new SeekException("Cannot seek").GetDiscordEmbed();
                    }
                }
                else
                {
                    builder = new SeekException("Nothing to seek").GetDiscordEmbed();
                }

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(builder);
                }
            }
        }
    }
}
