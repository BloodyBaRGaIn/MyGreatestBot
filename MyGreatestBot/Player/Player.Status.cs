using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void GetStatus(CommandActionSource source)
        {
            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }
            Handler.Message.Send(
                new PlayerStatusException(
                    $"Player current status is \"{Status.ToString().ToUpperInvariant()}\"")
                .WithSuccess());
        }
    }
}
