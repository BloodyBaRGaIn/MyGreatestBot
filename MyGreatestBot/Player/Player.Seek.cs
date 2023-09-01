using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void RequestSeek(TimeSpan span, CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (IsPlaying && currentTrack != null)
            {
                if (currentTrack.IsSeekPossible(span))
                {
                    lock (currentTrack)
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;
                    }

                    if (!mute)
                    {
                        Handler.Message.Send(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Purple,
                            Title = "Seek",
                            Description = currentTrack.GetMessage(),
                            Thumbnail = currentTrack.GetThumbnail()
                        });
                    }

                    Task.Yield().GetAwaiter().GetResult();
                }
                else
                {
                    if (!mute)
                    {
                        throw new SeekException("Cannot seek");
                    }
                    return;
                }
            }
            else
            {
                if (!mute)
                {
                    throw new SeekException("Nothing to seek");
                }
                return;
            }
        }
    }
}
