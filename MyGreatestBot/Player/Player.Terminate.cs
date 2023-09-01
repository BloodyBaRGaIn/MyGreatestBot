using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            Stop(source | CommandActionSource.Mute);
            MainPlayerCancellationTokenSource.Cancel();
        }
    }
}
