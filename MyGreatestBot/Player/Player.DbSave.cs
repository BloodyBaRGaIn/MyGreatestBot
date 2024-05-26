using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void DbSave(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            if (!DbSemaphore.TryWaitOne(1))
            {
                if (nomute)
                {
                    Handler.Message.Send(new DbSaveException("Operation in progress"));
                }
                return;
            }

            try
            {
                List<ITrackInfo> tracks = [];
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
                    if (nomute)
                    {
                        Handler.Message.Send(new DbSaveException("Nothing to save"));
                    }
                    return;
                }

                DbInstance.SaveTracks(tracks, Handler.GuildId);

                int tracksCount = tracks.Count;

                Stop(source | CommandActionSource.Mute);

                if (nomute)
                {
                    Handler.Message.Send(new DbSaveException($"Saved {tracksCount} track(s)").WithSuccess());
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
