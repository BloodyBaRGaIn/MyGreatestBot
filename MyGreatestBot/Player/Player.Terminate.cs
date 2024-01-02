using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            // player still restarts sometimes while closing app
            Stop(source | CommandActionSource.Mute);
            Wait(10);
            MainPlayerCancellationTokenSource.Cancel();
            Wait(10);
        }
    }
}
