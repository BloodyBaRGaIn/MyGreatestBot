using MyGreatestBot.Commands.Utils;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler : IDisposable
    {
        internal void Terminate(CommandActionSource source)
        {
            if ((Status & PlayerStatus.DeinitOrError) != PlayerStatus.None)
            {
                return;
            }
            Stop(source | CommandActionSource.Mute);
            MainPlayerCancellationTokenSource.Cancel();
            WaitForStatus(PlayerStatus.DeinitOrError);
            while (true)
            {
                if (MainPlayerTask == null
                    || MainPlayerTask.IsCompleted)
                {
                    break;
                }
            }
            try
            {
                MainPlayerTask?.Dispose();
            }
            catch { }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }
            disposed = true;
            Terminate(CommandActionSource.Event);
            if (disposing)
            {
                Handler.Dispose();
            }
        }

        ~PlayerHandler()
        {
            Dispose(false);
        }
    }
}
