using DicordNET.Commands;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0060")]
        internal static void Clear(CommandActionSource source = CommandActionSource.None)
        {
            lock (tracks_queue)
            {
                tracks_queue.Clear();
                IsPlaying = false;
                currentTrack = null;
            }
        }
    }
}
