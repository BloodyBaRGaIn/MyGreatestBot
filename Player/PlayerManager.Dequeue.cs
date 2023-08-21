using DicordNET.Bot;
using DicordNET.DB;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class PlayerManager
    {
        internal static void Dequeue()
        {
        get_track:

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
                goto get_track;
            }

            currentTrack = track;
        }
    }
}
