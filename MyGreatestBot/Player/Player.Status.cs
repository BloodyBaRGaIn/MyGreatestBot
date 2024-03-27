using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void GetStatus(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);
            if (nomute)
            {
                Handler.Message.Send(
                    new PlayerStatusException(
                        $"Player current status is \"{Status.ToString().ToUpperInvariant()}\"")
                    .WithSuccess());
            }
        }
    }
}
