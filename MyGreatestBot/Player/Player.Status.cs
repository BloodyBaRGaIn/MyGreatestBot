using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void GetStatus(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            messageHandler?.Send(new PlayerStatusException(
                    $"Player current status is \"{Status.ToString().ToUpperInvariant()}\"")
                .WithSuccess());
        }
    }
}
