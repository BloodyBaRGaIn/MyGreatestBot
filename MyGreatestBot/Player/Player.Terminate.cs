using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            Stop(source | CommandActionSource.Mute);
            while (true)
            {
                switch (Status)
                {
                    case PlayerStatus.Idle:
                        MainPlayerCancellationTokenSource.Cancel();
                        return;

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
