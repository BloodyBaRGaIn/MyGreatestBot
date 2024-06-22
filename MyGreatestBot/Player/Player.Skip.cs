using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Skip(int add_count, CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            if (tracksQueue.Count < add_count)
            {
                messageHandler?.Send(new SkipException("Requested number exceeds the queue length"));
                return;
            }

            lock (queueLock)
            {
                for (int i = 0; i < add_count; i++)
                {
                    _ = tracksQueue.Dequeue();
                }
            }

            IsPlaying = false;
            WaitForFinish();

            messageHandler?.Send(IsPlaying
                ? new SkipException($"Skipped{(add_count == 0 ? "" : $" {add_count + 1} tracks")}")
                    .WithSuccess()
                : new SkipException("Nothing to skip"));
        }
    }
}
