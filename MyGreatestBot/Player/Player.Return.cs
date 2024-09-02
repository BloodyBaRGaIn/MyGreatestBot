using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void ReturnCurrentTrackToQueue(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            bool success = true;

            lock (queueLock)
            {
                lock (trackLock)
                {
                    if (currentTrack == null)
                    {
                        success = false;
                    }
                    else if (source.HasFlag(CommandActionSource.PlayerToHead))
                    {
                        tracksQueue.EnqueueToHead(currentTrack);
                    }
                    else
                    {
                        currentTrack.PerformRewind(TimeSpan.Zero);
                        tracksQueue.Enqueue(currentTrack);
                    }
                }

                IsPlaying = false;
            }

            messageHandler?.Send(success
                ? new ReturnCommandException("Returned to queue").WithSuccess()
                : new ReturnCommandException("Nothing to return"));
        }
    }
}
