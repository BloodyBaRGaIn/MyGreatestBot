using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void DbRestore(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            if (!DbSemaphore.TryWaitOne(1))
            {
                if (nomute)
                {
                    Handler.Message.Send(new DbRestoreException("Operation in progress"));
                }
                return;
            }

            try
            {
                List<CompositeId> info;
                try
                {
                    info = DbInstance.RestoreTracks(Handler.GuildId);
                }
                catch (Exception ex)
                {
                    DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
                    if (nomute)
                    {
                        Handler.Message.Send(new DbRestoreException("Restore failed", ex));
                    }
                    return;
                }
                if (info.Count == 0)
                {
                    if (nomute)
                    {
                        Handler.Message.Send(new DbRestoreException("Nothing to restore"));
                    }
                    return;
                }
                int restoreCount = 0;
                foreach (CompositeId composite in info)
                {
                    ITrackInfo? track = ApiManager.Get<IMusicAPI>(composite.Api)?.GetTrackFromId(composite.Id);
                    if (track == null)
                    {
                        continue;
                    }
                    lock (queueLock)
                    {
                        tracksQueue.Enqueue(track);
                    }
                    Handler.Log.Send(track.GetShortMessage("Track restored"));
                    restoreCount++;
                }
                DbInstance.RemoveTracks(Handler.GuildId);
                if (nomute)
                {
                    Handler.Message.Send(new DbRestoreException($"Restored {restoreCount} track(s)").WithSuccess());
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _ = DbSemaphore.TryRelease();
            }
        }
    }
}
