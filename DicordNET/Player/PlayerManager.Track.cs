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

        internal static void GetNextTrackInfo()
        {
            if (tracks_queue.TryPeek(out var track))
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Next track",
                    Description = track.GetMessage(),
                    Thumbnail = track.GetThumbnail()
                });
            }
            else
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Next track",
                    Description = "Queue is empty"
                });
            }
        }
    }
}
