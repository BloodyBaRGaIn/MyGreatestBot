using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Skip(int add_count, CommandActionSource source)
        {
            DiscordEmbedBuilder builder;

            lock (queueLock)
            {
                if (tracksQueue.Count < add_count)
                {
                    builder = new SkipException("Requested number exceeds the queue length")
                        .GetDiscordEmbed();
                }
                else
                {
                    for (int i = 0; i < add_count; i++)
                    {
                        _ = tracksQueue.Dequeue();
                    }

                    builder = IsPlaying
                        ? new SkipException($"Skipped{(add_count == 0 ? "" : $" {add_count + 1} tracks")}")
                            .WithSuccess()
                            .GetDiscordEmbed()
                        : new SkipException("Nothing to skip").GetDiscordEmbed();

                    IsPlaying = false;
                    WaitForFinish();
                }

                if (!source.HasFlag(CommandActionSource.Mute))
                {
                    Handler.Message.Send(builder);
                }
            }
        }
    }
}
