using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Clear(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            lock (tracks_queue)
            {
                int count = tracks_queue.Count;
                tracks_queue.Clear();

                if (!mute)
                {
                    if (count > 0)
                    {
                        Handler.Message.Send(new ClearException("Queue cleared"), isSuccess: true);
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
