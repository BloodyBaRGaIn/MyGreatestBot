using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Clear(CommandActionSource source)
        {
            DiscordEmbedBuilder builder;

            lock (queueLock)
            {
                int count = tracksQueue.Count;
                tracksQueue.Clear();

                builder = count != 0
                    ? new ClearException("Queue cleared").WithSuccess().GetDiscordEmbed()
                    : new ClearException("Nothing to clear").GetDiscordEmbed();
            }

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                Handler.Message.Send(builder);
            }
        }
    }
}
