using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (IsPlaying || tracks_queue.Any())
            {
                StopRequested = true;

                Clear(source | CommandActionSource.Mute);

                IsPlaying = false;
                currentTrack = null;

                if (!mute)
                {
                    Handler.Message.Send(new StopException("Stopped"), true);
                }
            }
            else
            {
                if (!mute && !source.HasFlag(CommandActionSource.Event))
                {
                    throw new StopException("Nothing to stop");
                }
            }
        }
    }
}
