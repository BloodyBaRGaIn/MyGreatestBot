using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void GetCurrentTrackInfo()
        {
            DiscordEmbedBuilder builder;
            lock (trackLock)
            {
                if (currentTrack != null)
                {
                    builder = new TrackInfoCommandException(currentTrack.GetMessage("Current"))
                        .WithSuccess().GetDiscordEmbed();
                    builder.Thumbnail = currentTrack.Thumbnail;
                }
                else
                {
                    builder = (!IsPlaying
                        ? new TrackInfoCommandException("No tracks playing")
                        : new TrackInfoCommandException("Illegal state detected"))
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

                        builder = new TrackInfoCommandException(track.GetMessage("Next"))
                            .WithSuccess().GetDiscordEmbed();
                        builder.Thumbnail = track.Thumbnail;
                    }
                    else
                    {
                        builder = new TrackInfoCommandException("Tracks queue is empty").GetDiscordEmbed();
                    }
                    break;
                }

                Handler.Message.Send(builder);
            }
        }
    }
}
