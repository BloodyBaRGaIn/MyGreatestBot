using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void RequestSeek(TimeSpan span)
        {
            if (IsPlaying && currentTrack != null)
            {
                if (currentTrack.IsSeekPossible(span))
                {
                    lock (currentTrack)
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;
                    }

                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Purple,
                        Title = "Seek",
                        Description = currentTrack.GetMessage(),
                        Thumbnail = currentTrack.GetThumbnail()
                    });

                    Task.Yield().GetAwaiter().GetResult();
                }
                else
                {
                    throw new SeekException("Cannot seek");
                }
            }
            else
            {
                throw new SeekException("Nothing to seek");
            }
        }
    }
}
