using DicordNET.Bot;
using DicordNET.Sql;
using DSharpPlus.Entities;
using System.Runtime.Versioning;

namespace DicordNET.Player
{
    [SupportedOSPlatform("windows")]
    internal partial class PlayerManager
    {
        internal static void Dequeue()
        {
            while (true)
            {
                if (!tracks_queue.TryDequeue(out var track))
                {
                    currentTrack = null;
                    return;
                }

                if (SqlServerWrapper.IsTrackIgnored(track))
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping ignored track"
                    });
                    continue;
                }

                if (SqlServerWrapper.IsAnyArtistIgnored(track))
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
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
