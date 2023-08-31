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
                StopRequested = true;

                Clear(CommandActionSource.Mute);

                IsPlaying = false;
                currentTrack = null;

                if (!source.HasFlag(CommandActionSource.Mute))
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
                if (!source.HasFlag(CommandActionSource.Mute) && !source.HasFlag(CommandActionSource.External))
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Nothing to stop"
                    });
                }
            }
        }
    }
}
