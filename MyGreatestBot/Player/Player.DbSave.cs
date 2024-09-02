using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void DbSave(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            if (!DbSemaphore.TryWaitOne(1))
            {
                messageHandler?.Send(new DbSaveCommandException("Operation in progress"));
                return;
            }

            List<BaseTrackInfo> tracks = [];
            lock (trackLock)
            {
                if (currentTrack != null)
                {
                    tracks.Add(currentTrack);
                }
            }

            lock (queueLock)
            {
#pragma warning disable CS8620
                tracks.AddRange(tracksQueue.Where(t => t != null));
#pragma warning restore CS8620
            }

            if (tracks.Count == 0)
            {
                messageHandler?.Send(new DbSaveCommandException("Nothing to save"));
                return;
            }

            int tracksCount = tracks.Count;

            try
            {
                DbInstance.SaveTracks(tracks, Handler.GuildId);
            }
            catch (Exception ex)
            {
                messageHandler?.Send(new DbSaveCommandException("Cannot save tracks", ex));
                return;
            }
            finally
            {
                _ = DbSemaphore.TryRelease();
            }

            Stop(source | CommandActionSource.Mute);

            messageHandler?.Send(new DbSaveCommandException($"Saved {tracksCount} track(s)").WithSuccess());
        }

        internal void DbGetSavedCount(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            if (!DbSemaphore.TryWaitOne(1))
            {
                messageHandler?.Send(new DbSaveCommandException("Operation in progress"));
                return;
            }

            int tracksCount;

            try
            {
                tracksCount = DbInstance.GetTracksCount(Handler.GuildId);
            }
            catch (Exception ex)
            {
                messageHandler?.Send(new DbSaveCommandException("Cannot get saved tracks count", ex));
                return;
            }
            finally
            {
                _ = DbSemaphore.TryRelease();
            }

            if (tracksCount > 0)
            {
                messageHandler?.Send(new DbGetSavedCommandException(
                    $"{tracksCount} saved track(s) found").WithSuccess());
            }
            else
            {
                messageHandler?.Send(new DbGetSavedCommandException("No tracks saved"));
            }
        }
    }
}
