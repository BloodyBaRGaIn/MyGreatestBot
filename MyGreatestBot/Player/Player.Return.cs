using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void ReturnCurrentTrackToQueue(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (currentTrack == null)
            {
                if (!mute)
                {
                    throw new ReturnException("Nothing to return");
                }
                return;
            }

            lock (tracks_queue)
            {
                lock (currentTrack)
                {
                    currentTrack.PerformSeek(TimeSpan.Zero);
                    tracks_queue.Enqueue(currentTrack);
                }

                IsPlaying = false;

                if (!mute)
                {
                    Handler.Message.Send(new ReturnException("Returned to queue"), true);
                }
            }
        }
    }
}
