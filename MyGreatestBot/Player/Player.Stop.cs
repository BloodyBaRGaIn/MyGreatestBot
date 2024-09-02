using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            if (!IsPlaying && tracksQueue.Count == 0)
            {
                messageHandler?.Send(new StopCommandException("Nothing to stop"));
                return;
            }

            StopRequested = true;
            IsPlaying = false;
            WaitForStatus(PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);

            messageHandler?.Send(new StopCommandException("Stopped").WithSuccess());
        }
    }
}
