using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
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

                if (track == null)
                {
                    continue;
                }

                DiscordEmbedBuilder builder;

                try
                {
                    builder = GetPlayingMessage<PlayerException>(track, "Playing");
                }
                catch
                {
                    builder = new PlayerException("Cannot make playing message").GetDiscordEmbed();
                }

                Handler.Message.Send(builder);

                if (!track.BypassCheck)
                {
                    if (SqlServerWrapper.Instance.IsAnyArtistIgnored(track, Handler.GuildId))
                    {
                        Handler.Message.Send(new IgnoreException("Skipping track with ignored artist(s)").WithSuccess());
                        continue;
                    }

                    if (SqlServerWrapper.Instance.IsTrackIgnored(track, Handler.GuildId))
                    {
                        Handler.Message.Send(new IgnoreException("Skipping ignored track").WithSuccess());
                        continue;
                    }
                }

                if (track.Duration >= MaxTrackDuration)
                {
                    Handler.Message.Send(new IgnoreException("Track is too long").WithSuccess());
                    continue;
                }

                if (!track.IsLiveStream && track.Duration <= MinTrackDuration)
                {
                    Handler.Message.Send(new IgnoreException("Track is too short").WithSuccess());
                    continue;
                }

                currentTrack = track;
                break;
            }
        }
    }
}
