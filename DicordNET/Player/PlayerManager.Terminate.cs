using DicordNET.Commands;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Terminate(CommandActionSource source = CommandActionSource.None)
        {
            MainPlayerCancellationTokenSource.Cancel();
            Clear(source);
        }
    }
}
