using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Pause(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            IsPaused = true;
            WaitForStatus(PlayerStatus.Paused | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);

            messageHandler?.Send(currentTrack != null
                ? new PauseException("Paused").WithSuccess()
                : !IsPlaying
                ? new PauseException("Nothing to pause")
                : new PlayerException("Illegal state detected"));
        }
    }
}
