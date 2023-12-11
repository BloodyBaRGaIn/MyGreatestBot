using MyGreatestBot.Commands.Utils;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Terminate(CommandActionSource source)
        {
            Stop(source | CommandActionSource.Mute);
            Task.Yield().GetAwaiter().GetResult();
            MainPlayerCancellationTokenSource.Cancel();
            Task.Yield().GetAwaiter().GetResult();
        }
    }
}
