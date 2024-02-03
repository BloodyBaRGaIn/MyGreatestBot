using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (IsPlaying || tracks_queue.Count != 0)
            {
                StopRequested = true;

                Clear(source | CommandActionSource.Mute);

                IsPlaying = false;

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
