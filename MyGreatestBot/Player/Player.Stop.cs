using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Stop(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            if (IsPlaying || tracks_queue.Count != 0)
            {
                StopRequested = true;

                Clear(source | CommandActionSource.Mute);

                IsPlaying = false;

                if (nomute)
                {
                    Handler.Message.Send(new StopException("Stopped").WithSuccess());
                }
            }
            else
            {
                if (nomute && !source.HasFlag(CommandActionSource.Event))
                {
                    throw new StopException("Nothing to stop");
                }
            }
        }
    }
}
