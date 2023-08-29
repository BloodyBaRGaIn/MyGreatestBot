using DSharpPlus.Entities;
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
                bool result = currentTrack.IsSeekPossible(span);

                if (result)
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
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Seek",
                        Description = "Cannot seek"
                    });

                    return;
                }
            }
            else
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Seek",
                    Description = "Nothing to seek"
                });

                return;
            }
        }
    }
}
