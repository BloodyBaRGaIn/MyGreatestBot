using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Stop(CommandActionSource source = CommandActionSource.None)
        {
            if (IsPlaying || tracks_queue.Any())
            {
                if ((source & CommandActionSource.External) != 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Kicked from voice channel"
                    });
                    return;
                }

                Clear(source);

                if ((source & CommandActionSource.Mute) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Stopped"
                    });
                }
            }
            else
            {
                if ((source & (CommandActionSource.Mute | CommandActionSource.External)) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to stop"
                    });
                }
            }
        }
    }
}
