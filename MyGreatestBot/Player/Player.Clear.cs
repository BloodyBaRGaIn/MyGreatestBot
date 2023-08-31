using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Clear(CommandActionSource source = CommandActionSource.None)
        {
            lock (tracks_queue)
            {
                int count = tracks_queue.Count;
                tracks_queue.Clear();

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    if (count > 0)
                    {
                        Handler.SendMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Aquamarine,
                            Title = "Clear",
                            Description = "Queue cleared"
                        });
                    }
                    else
                    {
                        throw new ClearException("Nothing to clear");
                    }
                }
            }
        }
    }
}
