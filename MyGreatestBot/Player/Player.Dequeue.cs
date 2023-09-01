using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Sql;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
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

                Handler.Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Purple,
                    Title = "Play",
                    Description = track.GetMessage(),
                    Thumbnail = track.GetThumbnail()
                });

                if (SqlServerWrapper.IsAnyArtistIgnored(track))
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping track with ignored artist(s)"
                    });
                    continue;
                }

                if (SqlServerWrapper.IsTrackIgnored(track))
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping ignored track"
                    });
                    continue;
                }

                if (track.Duration >= MaxTrackDuration)
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Track is too long"
                    });
                    continue;
                }

                currentTrack = track;
                break;
            }
        }
    }
}
