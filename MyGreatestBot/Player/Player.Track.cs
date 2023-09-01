using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void GetCurrentTrackInfo()
        {
            if (currentTrack != null)
            {
                lock (currentTrack)
                {
                    DiscordEmbedBuilder message = new TrackInfoException(currentTrack.GetMessage("Current")).GetDiscordEmbed(true);
                    message.Thumbnail = currentTrack.GetThumbnail();

                    Handler.Message.Send(message);
                }
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
                    DiscordEmbedBuilder message = new TrackInfoException(track.GetMessage("Next")).GetDiscordEmbed(true);
                    message.Thumbnail = track.GetThumbnail();

                    Handler.Message.Send(message);
                }
                else
                {
                    throw new TrackInfoException("Tracks queue is empty");
                }
            }
        }
    }
}
