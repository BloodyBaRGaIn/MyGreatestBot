using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Pause(CommandActionSource source)
        {
            IsPaused = true;

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (currentTrack != null)
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Paused"
                    });
                }
                else if (!IsPlaying)
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
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
