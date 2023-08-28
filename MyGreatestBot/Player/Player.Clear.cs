using MyGreatestBot.Commands;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Clear(CommandActionSource source = CommandActionSource.None)
        {
            _ = source;
            lock (tracks_queue)
            {
                tracks_queue.Clear();
                IsPlaying = false;
                currentTrack = null;
            }
        }
    }
}
