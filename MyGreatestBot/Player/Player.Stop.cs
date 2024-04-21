using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            DiscordEmbedBuilder builder;

            if (IsPlaying || tracksQueue.Count != 0)
            {
                builder = new StopException("Stopped").WithSuccess().GetDiscordEmbed();
                StopRequested = true;
                IsPlaying = false;
                WaitForStatus(PlayerStatus.InitOrIdle | PlayerStatus.DeinitOrError);
            }
            else
            {
                builder = new StopException("Nothing to stop").GetDiscordEmbed();
            }

            if (!source.HasFlag(CommandActionSource.Mute))
            {
                Handler.Message.Send(builder);
            }
        }
    }
}
