using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;

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
                    Handler.Message.Send(GetPlayingMessage<TrackInfoException>(currentTrack, "Current"));
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
                        Handler.Message.Send(GetPlayingMessage<TrackInfoException>(track, "Next"));
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
