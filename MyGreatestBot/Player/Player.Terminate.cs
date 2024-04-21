using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            Stop(source | CommandActionSource.Mute);
            MainPlayerCancellationTokenSource.Cancel();
            WaitForStatus(PlayerStatus.DeinitOrError);
            while (true)
            {
                if (MainPlayerTask == null
                    || MainPlayerTask.IsCompletedSuccessfully
                    || MainPlayerTask.IsCompleted
                    || MainPlayerTask.IsCanceled
                    || MainPlayerTask.IsFaulted)
                {
                    break;
                }
            }
        }
    }
}
