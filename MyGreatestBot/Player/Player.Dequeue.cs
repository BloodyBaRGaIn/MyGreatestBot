using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal sealed partial class Player
    {
        private void Dequeue()
        {
            while (true)
            {
                if (!tracks_queue.TryDequeue(out ITrackInfo? track))
                {
                    currentTrack = null;
                    return;
                }

                Handler.Message.Send(GetPlayingMessage<PlayerException>(track, "Playing"));

                if (SqlServerWrapper.IsAnyArtistIgnored(track))
                {
                    Handler.Message.Send(new IgnoreException("Skipping track with ignored artist(s)"), true);
                    continue;
                }

                if (SqlServerWrapper.IsTrackIgnored(track))
                {
                    Handler.Message.Send(new IgnoreException("Skipping ignored track"), true);
                    continue;
                }

                if (track.Duration >= MaxTrackDuration)
                {
                    Handler.Message.Send(new IgnoreException("Track is too long"), true);
                    continue;
                }

                currentTrack = track;
                break;
            }
        }
    }
}
