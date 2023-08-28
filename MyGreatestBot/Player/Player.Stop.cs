using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Stop(CommandActionSource source = CommandActionSource.None)
        {
            if (IsPlaying || tracks_queue.Any())
            {
                if ((source & CommandActionSource.External) != 0)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Kicked from voice channel"
                    });
                    return;
                }

                Clear(source);

                if ((source & CommandActionSource.Mute) == 0)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
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
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to stop"
                    });
                }
            }
        }
    }
}
