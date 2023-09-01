using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Skip(int add_count, CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            lock (tracks_queue)
            {
                if (tracks_queue.Count < add_count)
                {
                    if (!mute)
                    {
                        throw new SkipException("Requested number exceeds the queue length");
                    }
                    return;
                }
                for (int i = 0; i < add_count; i++)
                {
                    _ = tracks_queue.Dequeue();
                }

                bool was_playing = IsPlaying;

                currentTrack = null;
                IsPlaying = false;

                if (!mute)
                {
                    if (was_playing)
                    {
                        Handler.Message.Send(new SkipException($"Skipped{(add_count == 0 ? "" : $" {add_count + 1} tracks")}"), true);
                    }
                    else
                    {
                        Handler.Message.Send(new SkipException("Nothing to skip"));
                    }
                }
            }
        }
    }
}
