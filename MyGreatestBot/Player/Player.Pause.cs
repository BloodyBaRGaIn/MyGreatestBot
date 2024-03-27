using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Pause(CommandActionSource source)
        {
            IsPaused = true;
            WaitForPause();
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

        private void WaitForPause()
        {
            while (true)
            {
                switch (Status)
                {
                    case PlayerStatus.Paused:
                    case PlayerStatus.Idle:
                    case PlayerStatus.Deinit:
                    case PlayerStatus.Error:
                        return;

                    default:
                        Wait();
                        break;
                }
            }
        }
    }
}
