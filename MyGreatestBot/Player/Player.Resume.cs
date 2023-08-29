using DSharpPlus.Entities;
using MyGreatestBot.Commands;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Resume(CommandActionSource source = CommandActionSource.None)
        {
            if (!source.HasFlag(CommandActionSource.Mute))
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
