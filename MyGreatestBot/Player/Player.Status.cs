using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void GetStatus(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            messageHandler?.Send(new PlayerStatusCommandException(
                    $"Player current status is \"{Status.ToString().ToUpperInvariant()}\"")
                .WithSuccess());
        }
    }
}
