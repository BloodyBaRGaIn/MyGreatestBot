using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {

        internal void GetUniqueTracks(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            if (tracksQueue.Count == 0)
            {
                messageHandler?.Send(new UniqueCommandException("Queue is empty"));
                return;
            }

            DiscordEmbedBuilder builder;
            int originCount = tracksQueue.Count;

            lock (queueLock)
            {
                List<BaseTrackInfo> collection = [];
                while (tracksQueue.Count != 0)
                {
                    BaseTrackInfo? track = tracksQueue.Dequeue();
                    if (track != null && (currentTrack == null || !currentTrack.Equals(track)))
                    {
                        collection.Add(track);
                    }
                }
                collection = [.. collection.DistinctBy(static track => track.TrackName.InnerId)];
                tracksQueue.EnqueueRange(collection);

                builder = new UniqueCommandException(
                    (originCount == tracksQueue.Count)
                    ? "Queue does not contain repeats"
                    : $"{originCount - tracksQueue.Count} tracks removed")
                        .WithSuccess().GetDiscordEmbed();
            }

            messageHandler?.Send(builder);
        }
    }
}
