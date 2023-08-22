using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Pause(CommandActionSource source = CommandActionSource.None)
        {
            if ((source & CommandActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Paused"
                    });
                }
                else
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Nothing to pause"
                    });
                }
            }

            IsPaused = true;
        }
    }
}
