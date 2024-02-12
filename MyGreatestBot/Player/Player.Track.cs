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
            if (currentTrack != null)
            {
                lock (currentTrack)
                {
                    DiscordEmbedBuilder builder;
                    try
                    {
                        builder = GetPlayingMessage<TrackInfoException>(currentTrack, "Current");
                    }
                    catch
                    {
                        builder = new TrackInfoException("Cannot make track message").GetDiscordEmbed();
                    }
                    Handler.Message.Send(builder);
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
                while (true)
                {
                    if (tracks_queue.TryPeek(out ITrackInfo? track))
                    {
                        if (track == null)
                        {
                            _ = tracks_queue.Dequeue();
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
                        break;
                    }
                    else
                    {
                        throw new TrackInfoException("Tracks queue is empty");
                    }
                }
            }
        }
    }
}
