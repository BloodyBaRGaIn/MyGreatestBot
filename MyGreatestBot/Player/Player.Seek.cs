using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void RequestSeek(TimeSpan span, CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            lock (trackLock)
            {
                if (IsPlaying && currentTrack != null)
                {
                    if (currentTrack.IsSeekPossible(span))
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;

                        if (nomute)
                        {
                            DiscordEmbedBuilder builder;
                            try
                            {
                                builder = GetPlayingMessage<SeekException>(currentTrack, "Playing");
                            }
                            catch
                            {
                                builder = new SeekException("Cannot make seek message").GetDiscordEmbed();
                            }
                            Handler.Message.Send(builder);
                        }
                    }
                    else
                    {
                        if (nomute)
                        {
                            Handler.Message.Send(new SeekException("Cannot seek"));
                        }
                        return;
                    }
                }
                else
                {
                    if (nomute)
                    {
                        Handler.Message.Send(new SeekException("Nothing to seek"));
                    }
                    return;
                }
            }
        }
    }
}
