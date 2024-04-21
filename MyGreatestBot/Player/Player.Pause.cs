using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Pause(CommandActionSource source)
        {
            IsPaused = true;
            WaitForStatus(PlayerStatus.Paused | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);

            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }

            Handler.Message.Send(currentTrack != null
                ? new PauseException("Paused").WithSuccess().GetDiscordEmbed()
                : !IsPlaying
                ? new PauseException("Nothing to pause").GetDiscordEmbed()
                : new PlayerException("Illegal state detected").GetDiscordEmbed());
        }
    }
}
