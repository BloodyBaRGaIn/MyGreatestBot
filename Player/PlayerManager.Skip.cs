using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Skip(CommandActionSource source = CommandActionSource.None)
        {
            if ((source & CommandActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Skipped"
                    });
                }
                else
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to skip"
                    });
                }
            }
            IsPlaying = false;
        }
    }
}
