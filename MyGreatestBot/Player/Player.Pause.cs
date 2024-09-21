using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void Pause(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            WaitForStatus(PlayerStatus.Paused | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError,
                          () =>
                          {
                              IsPaused = true;
                          });

            messageHandler?.Send(currentTrack != null
                ? new PauseCommandException("Paused").WithSuccess()
                : !IsPlaying
                ? new PauseCommandException("Nothing to pause")
                : new PlayerException("Illegal state detected"));
        }
    }
}
