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
                    try
                    {
                        builder = GetPlayingMessage<TrackInfoException>(currentTrack, "Current");
                    }
                    catch
                    {
                        builder = new TrackInfoException("Cannot make track message").GetDiscordEmbed();
                    }
                }
                else
                {
                    builder = !IsPlaying
                        ? new TrackInfoException("No tracks playing").GetDiscordEmbed()
                        : new TrackInfoException("Illegal state detected").GetDiscordEmbed();
                }

                Handler.Message.Send(builder);
            }
        }

        internal void GetNextTrackInfo()
        {
            lock (queueLock)
            {
                DiscordEmbedBuilder builder;

                while (true)
                {
                    if (tracksQueue.TryPeek(out ITrackInfo? track))
                    {
                        if (track == null)
                        {
                            _ = tracksQueue.Dequeue();
                            continue;
                        }

                        try
                        {
                            builder = GetPlayingMessage<TrackInfoException>(track, "Next");
                        }
                        catch
                        {
                            builder = new TrackInfoException("Cannot make next track message").GetDiscordEmbed();
                        }
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
