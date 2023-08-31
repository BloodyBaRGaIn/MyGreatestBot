using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void GetCurrentTrackInfo()
        {
            if (currentTrack != null)
            {
                DiscordEmbedBuilder message = new()
                {
                    Color = DiscordColor.Purple,
                    Title = "Track"
                };
                lock (currentTrack)
                {
                    message.Description = currentTrack.GetMessage();
                    message.Thumbnail = currentTrack.GetThumbnail();
                }
                Handler.SendMessage(message);
            }
            else if (!IsPlaying)
            {
                throw new TrackInfoException("No tracks playing");
            }
            else
            {
                throw new TrackInfoException("Illegal state detected");
            }
        }

        internal void GetNextTrackInfo()
        {
            lock (tracks_queue)
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
}
