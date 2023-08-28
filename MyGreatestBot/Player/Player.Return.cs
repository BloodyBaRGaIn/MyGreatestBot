using DSharpPlus.Entities;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void ReturnCurrentTrackToQueue()
        {
            if (currentTrack == null)
            {
                Handler.SendMessage(new DiscordEmbedBuilder()
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

                Handler.SendMessage(new DiscordEmbedBuilder()
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
