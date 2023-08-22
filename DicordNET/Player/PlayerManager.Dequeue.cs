using DicordNET.Bot;
using DicordNET.DB;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
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

                if (DataBaseManager.IsIgnored(track))
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping ignored track"
                    });
                    continue;
                }

                if (DataBaseManager.IsArtistIgnored(track))
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
