using DicordNET.Bot;
using DicordNET.Commands;
using DicordNET.DB;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class PlayerManager
    {
        internal static void Ignore(CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to ignore"
                    });
                }

                return;
            }

            DataBaseManager.AddIgnoredTrack(currentTrack);

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track ignored"
                });
            }
        }
    }
}
