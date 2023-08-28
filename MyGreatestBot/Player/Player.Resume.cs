using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class Player
    {
        internal void Resume(CommandActionSource source = CommandActionSource.None)
        {
            if ((source & CommandActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Resumed"
                    });
                }
                else
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Green,
                        Title = "Nothing to resume"
                    });
                }
            }

            IsPaused = false;
        }
    }
}
