using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Resume(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            IsPaused = false;
            WaitForStatus(PlayerStatus.Playing | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);

            messageHandler?.Send(currentTrack == null
                ? new ResumeException("Nothing to resume")
                : IsPlaying
                ? new ResumeException("Resumed").WithSuccess()
                : new PlayerException("Illegal state detected"));
        }
    }
}
