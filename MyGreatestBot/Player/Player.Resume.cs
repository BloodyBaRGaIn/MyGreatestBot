using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Resume(CommandActionSource source)
        {
            IsPaused = false;
            WaitForResume();
            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }
            if (currentTrack == null)
            {
                throw new ResumeException("Nothing to resume");
            }
            else if (IsPlaying)
            {
                Handler.Message.Send(new ResumeException("Resumed").WithSuccess());
            }
            else
            {
                throw new PlayerException("Illegal state detected");
            }
        }

        private void WaitForResume()
        {
            while (true)
            {
                switch (Status)
                {
                    case PlayerStatus.Playing:
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
