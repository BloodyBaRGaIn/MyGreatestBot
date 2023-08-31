using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using System;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void ReturnCurrentTrackToQueue()
        {
            if (currentTrack == null)
            {
                throw new ReturnException("Nothing to return");
            }

            lock (tracks_queue)
            {
                lock (currentTrack)
                {
                    currentTrack.PerformSeek(TimeSpan.Zero);
                    tracks_queue.Enqueue(currentTrack);
                }

                IsPlaying = false;

                Handler.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Return",
                    Description = "Returned to queue"
                });
            }
        }
    }
}
