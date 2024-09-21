using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void Resume(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            WaitForStatus(PlayerStatus.Playing | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError,
                          () =>
                          {
                              IsPaused = false;
                          });

            messageHandler?.Send(currentTrack == null
                ? new ResumeCommandException("Nothing to resume")
                : IsPlaying
                ? new ResumeCommandException("Resumed").WithSuccess()
                : new PlayerException("Illegal state detected"));
        }
    }
}
