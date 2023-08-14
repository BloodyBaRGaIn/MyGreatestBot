using DicordNET.Bot;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void GetCurrentTrackInfo()
        {
            if (IsPlaying && currentTrack != null)
            {
                lock (currentTrack)
                {
                    currentTrack.PerformSeek(Seek);
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Purple,
                        Title = "Track",
                        Description = currentTrack.GetMessage(),
                        Thumbnail = currentTrack.GetThumbnail()
                    });
                }
            }
            else
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Track",
                    Description = "No tracks playing"
                });
            }
        }
    }
}
