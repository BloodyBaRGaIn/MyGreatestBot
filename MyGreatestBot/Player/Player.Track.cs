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
                else if (!IsPlaying)
                {
                    builder = new TrackInfoException("No tracks playing").GetDiscordEmbed();
                }
                else
                {
                    builder = new TrackInfoException("Illegal state detected").GetDiscordEmbed();
                }
                Handler.Message.Send(builder);
            }
        }

        internal void GetNextTrackInfo()
        {
            lock (queueLock)
            {
                while (true)
                {
                    if (tracksQueue.TryPeek(out ITrackInfo? track))
                    {
                        if (track == null)
                        {
                            _ = tracksQueue.Dequeue();
                            continue;
                        }
                        DiscordEmbedBuilder builder;
                        try
                        {
                            builder = GetPlayingMessage<TrackInfoException>(track, "Next");
                        }
                        catch
                        {
                            builder = new TrackInfoException("Cannot make next track message").GetDiscordEmbed();
                        }
                        Handler.Message.Send(builder);
                    }
                    else
                    {
                        Handler.Message.Send(new TrackInfoException("Tracks queue is empty"));
                    }
                    break;
                }
            }
        }
    }
}
