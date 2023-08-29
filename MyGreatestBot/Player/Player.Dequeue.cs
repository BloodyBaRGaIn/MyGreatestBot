using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Sql;
using System.Runtime.Versioning;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        internal void Dequeue()
        {
            while (true)
            {
                if (!tracks_queue.TryDequeue(out ITrackInfo? track))
                {
                    currentTrack = null;
                    return;
                }

                if (SqlServerWrapper.IsAnyArtistIgnored(track))
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping track with ignored artist(s)"
                    });
                    continue;
                }

                if (SqlServerWrapper.IsTrackIgnored(track))
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping ignored track"
                    });
                    continue;
                }

                currentTrack = track;
                break;
            }
        }
    }
}
