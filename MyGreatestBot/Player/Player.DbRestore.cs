using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.Player
{
    internal sealed partial class PlayerHandler
    {
        internal void DbRestore(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            if (!DbSemaphore.TryWaitOne(1))
            {
                messageHandler?.Send(new DbRestoreCommandException("Operation in progress"));
                return;
            }

            List<CompositeId> info;
            try
            {
                info = DbInstance.RestoreTracks(Handler.GuildId);
            }
            catch (Exception ex)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
                messageHandler?.Send(new DbRestoreCommandException("Restore failed", ex));
                return;
            }

            if (info.Count == 0)
            {
                messageHandler?.Send(new DbRestoreCommandException("Nothing to restore"));
                return;
            }

            Exception? last_exception = null;
            int restoreCount = 0;

            try
            {
                foreach (CompositeId composite in info)
                {
                    BaseTrackInfo? track = ApiManager.GetMusicApiInstance(composite.Api)
                        ?.GetTrackFromId(composite.Id);

                    if (track == null)
                    {
                        continue;
                    }
                    lock (queueLock)
                    {
                        tracksQueue.Enqueue(track);
                    }
                    Handler.Log.Send(track.GetMessage("Track restored", shortMessage: true));
                    restoreCount++;
                }
                DbInstance.RemoveTracks(Handler.GuildId);
            }
            catch (Exception ex)
            {
                last_exception = ex;
            }
            finally
            {
                _ = DbSemaphore.TryRelease();
            }

            messageHandler?.Send(last_exception != null
                ? new DbRestoreCommandException("Cannot restore tracks", last_exception)
                : new DbRestoreCommandException($"Restored {restoreCount} track(s)").WithSuccess());
        }
    }
}
