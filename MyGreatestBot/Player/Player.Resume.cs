using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Resume(CommandActionSource source)
        {
            IsPaused = false;
            WaitForStatus(PlayerStatus.Playing | PlayerStatus.Finish | PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);

            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }

            DiscordEmbedBuilder builder = currentTrack == null
                ? new ResumeException("Nothing to resume").GetDiscordEmbed()
                : IsPlaying
                ? new ResumeException("Resumed").WithSuccess().GetDiscordEmbed()
                : new PlayerException("Illegal state detected").GetDiscordEmbed();

            Handler.Message.Send(builder);
        }
    }
}
