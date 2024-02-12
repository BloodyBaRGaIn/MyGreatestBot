using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Clear(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            lock (tracks_queue)
            {
                int count = tracks_queue.Count;
                tracks_queue.Clear();

                if (nomute)
                {
                    if (count > 0)
                    {
                        Handler.Message.Send(new ClearException("Queue cleared").WithSuccess());
                    }
                    else
                    {
                        throw new ClearException("Nothing to clear");
                    }
                }
            }
        }
    }
}
