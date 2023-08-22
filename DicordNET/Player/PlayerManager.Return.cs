using DicordNET.Bot;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void ReturnCurrentTrackToQueue()
        {
            if (currentTrack == null)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Red,
                    Title = "Return",
                    Description = "Nothing to return"
                });
            }
            else
            {
                lock (tracks_queue)
                {
                    lock (currentTrack)
                    {
                        tracks_queue.Enqueue(currentTrack);
                    }
                }

                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Return",
                    Description = "Returned to queue"
                });
            }

            IsPlaying = false;
        }
    }
}
