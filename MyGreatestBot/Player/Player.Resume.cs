using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Resume(CommandActionSource source)
        {
            IsPaused = false;
            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }
            if (currentTrack == null)
            {
                Handler.Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Green,
                    Title = "Nothing to resume"
                });
            }
            else if (IsPlaying)
            {
                Handler.Message.Send(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Green,
                    Title = "Resumed"
                });
            }
            else
            {
                throw new PlayerException("Illegal state detected");
            }
        }
    }
}
