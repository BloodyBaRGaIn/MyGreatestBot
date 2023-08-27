using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class Player
    {
        internal void GetCurrentTrackInfo()
        {
            if (IsPlaying && currentTrack != null)
            {
                lock (currentTrack)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
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
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Track",
                    Description = "No tracks playing"
                });
            }
        }

        internal void GetNextTrackInfo()
        {
            if (tracks_queue.TryPeek(out var track))
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Next track",
                    Description = track.GetMessage(),
                    Thumbnail = track.GetThumbnail()
                });
            }
            else
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Next track",
                    Description = "Queue is empty"
                });
            }
        }
    }
}
