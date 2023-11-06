using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal bool GetPausedState()
        {
            return IsPaused;
        }

        internal void Pause(CommandActionSource source)
        {
            IsPaused = true;

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (currentTrack != null)
                {
                    Handler.Message.Send(new PauseException("Paused"), true);
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
}
