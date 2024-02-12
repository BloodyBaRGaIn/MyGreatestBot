using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Pause(CommandActionSource source)
        {
            IsPaused = true;

            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }
            if (currentTrack != null)
            {
                Handler.Message.Send(new PauseException("Paused").WithSuccess());
            }
            else if (!IsPlaying)
            {
                throw new PauseException("Nothing to pause");
            }
            else
            {
                throw new PlayerException("Illegal state detected");
            }
        }
    }
}
