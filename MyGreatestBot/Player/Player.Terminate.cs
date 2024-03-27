using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            Stop(source | CommandActionSource.Mute);
            WaitForIdleOrError();
            if (Status == PlayerStatus.Idle)
            {
                MainPlayerCancellationTokenSource.Cancel();
            }
        }

        private void WaitForIdleOrError()
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
