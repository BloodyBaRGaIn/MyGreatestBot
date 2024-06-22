using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Clear(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            int count;

            lock (queueLock)
            {
                count = tracksQueue.Count;
                tracksQueue.Clear();
            }

            messageHandler?.Send(count != 0
                ? new ClearException("Queue cleared").WithSuccess()
                : new ClearException("Nothing to clear"));
        }
    }
}
