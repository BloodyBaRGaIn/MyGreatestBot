using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Clear(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            lock (queueLock)
            {
                int count = tracksQueue.Count;
                tracksQueue.Clear();

                if (nomute)
                {
                    if (count > 0)
                    {
                        Handler.Message.Send(new ClearException("Queue cleared").WithSuccess());
                    }
                    else
                    {
                        Handler.Message.Send(new ClearException("Nothing to clear"));
                    }
                }
            }
        }
    }
}
