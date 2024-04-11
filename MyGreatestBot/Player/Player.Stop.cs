using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            if (IsPlaying || tracksQueue.Count != 0)
            {
                StopRequested = true;
                IsPlaying = false;
                WaitForIdle();

                if (nomute)
                {
                    Handler.Message.Send(new StopException("Stopped").WithSuccess());
                }
            }
            else
            {
                if (nomute)
                {
                    Handler.Message.Send(new StopException("Nothing to stop"));
                }
            }
        }

        private void WaitForIdle()
        {
            while (true)
            {
                switch (Status)
                {
                    case PlayerStatus.Idle:
                    case PlayerStatus.Init:
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
