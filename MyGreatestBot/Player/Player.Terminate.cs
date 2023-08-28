using MyGreatestBot.Commands;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Terminate(CommandActionSource source = CommandActionSource.None)
        {
            MainPlayerCancellationTokenSource.Cancel();
            Clear(source);
        }
    }
}
