using DicordNET.Sql;
using DSharpPlus.Entities;
using System.Runtime.Versioning;

namespace DicordNET.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class Player
    {
        internal void Dequeue()
        {
            while (true)
            {
                if (!tracks_queue.TryDequeue(out ApiClasses.ITrackInfo? track))
                {
                    currentTrack = null;
                    return;
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

                if (SqlServerWrapper.IsAnyArtistIgnored(track))
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping track with ignored artist(s)"
                    });
                    continue;
                }

                currentTrack = track;
                break;
            }
        }
    }
}
