using DSharpPlus.Entities;
using MyGreatestBot.Commands.Utils;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (IsPlaying || tracks_queue.Any())
            {
                StopRequested = true;

                Clear(CommandActionSource.Mute);

                IsPlaying = false;
                currentTrack = null;

                if (!mute)
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Stopped"
                    });
                }
            }
            else
            {
                if (!mute && !source.HasFlag(CommandActionSource.Event))
                {
                    Handler.Message.Send(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = "Nothing to stop"
                    });
                }
            }
        }
    }
}
