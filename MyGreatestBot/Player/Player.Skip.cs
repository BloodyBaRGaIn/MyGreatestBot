using DSharpPlus.Entities;
using MyGreatestBot.Commands;
using MyGreatestBot.Commands.Exceptions;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Skip(int add_count = 0, CommandActionSource source = CommandActionSource.None)
        {
            lock (tracks_queue)
            {
                if (tracks_queue.Count < add_count)
                {
                    if (!source.HasFlag(CommandActionSource.Mute))
                    {
                        throw new SkipException("Requested number exceeds the queue length");
                    }
                    return;
                }
                for (int i = 0; i < add_count; i++)
                {
                    _ = tracks_queue.Dequeue();
                }
            }

            currentTrack = null;
            IsPlaying = false;

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (IsPlaying)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = $"Skipped{(add_count == 0 ? "" : $" {add_count + 1} tracks")}"
                    });
                }
                else
                {
                    throw new SkipException("Nothing to skip");
                }
            }
        }
    }
}
