using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Resume(CommandActionSource source = CommandActionSource.None)
        {
            IsPaused = false;
            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (currentTrack == null)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Nothing to resume"
                    });
                }
                else if (IsPlaying)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
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
}
