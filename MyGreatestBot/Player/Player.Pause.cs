using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Pause(CommandActionSource source = CommandActionSource.None)
        {
            IsPaused = true;

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (currentTrack != null)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Paused"
                    });
                }
                else if (!IsPlaying)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Nothing to pause"
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
