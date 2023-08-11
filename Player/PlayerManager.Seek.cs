using DicordNET.Bot;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void RequestSeek(TimeSpan span)
        {
            if (IsPlaying && currentTrack != null)
            {
                bool result = currentTrack.TrySeek(span);

                if (result)
                {
                    lock (currentTrack)
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;
                    }

                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
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
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
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
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
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
