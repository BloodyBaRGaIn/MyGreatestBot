using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
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
                throw new TrackInfoException("No tracks playing");
            }
        }

        internal void GetNextTrackInfo()
        {
            if (tracks_queue.TryPeek(out ITrackInfo? track))
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
                throw new TrackInfoException("Tracks queue is empty");
            }
        }
    }
}
