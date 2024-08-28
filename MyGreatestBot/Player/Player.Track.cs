using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void GetCurrentTrackInfo()
        {
            DiscordEmbedBuilder builder;
            lock (trackLock)
            {
                if (currentTrack != null)
                {
                    builder = new TrackInfoException(currentTrack.GetMessage("Current"))
                        .WithSuccess().GetDiscordEmbed();
                    builder.Thumbnail = currentTrack.GetThumbnail();
                }
                else
                {
                    builder = (!IsPlaying
                        ? new TrackInfoException("No tracks playing")
                        : new TrackInfoException("Illegal state detected"))
                        .GetDiscordEmbed();
                }
            }

            Handler.Message.Send(builder);
        }

        internal void GetNextTrackInfo()
        {
            lock (queueLock)
            {
                DiscordEmbedBuilder builder;

                while (true)
                {
                    if (tracksQueue.TryPeek(out BaseTrackInfo? track))
                    {
                        if (track == null)
                        {
                            _ = tracksQueue.Dequeue();
                            continue;
                        }

                        builder = new TrackInfoException(track.GetMessage("Next"))
                            .WithSuccess().GetDiscordEmbed();
                        builder.Thumbnail = track.GetThumbnail();
                    }
                    else
                    {
                        builder = new TrackInfoException("Tracks queue is empty").GetDiscordEmbed();
                    }
                    break;
                }

                Handler.Message.Send(builder);
            }
        }
    }
}
