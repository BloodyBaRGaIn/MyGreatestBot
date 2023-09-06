using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Sql;
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
        internal void Save(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!_sqlSemaphore.WaitOne(1))
            {
                if (!mute)
                {
                    throw new SaveException("Operation in progress");
                }
                return;
            }

            try
            {
                if (!tracks_queue.Any())
                {
                    if (!mute)
                    {
                        throw new SaveException("Nothing to save");
                    }
                    return;
                }
                lock (tracks_queue)
                {
                    SqlServerWrapper.RemoveTracks(Handler.GuildId);

                    List<ITrackInfo> tracks = new();
                    if (currentTrack != null)
                    {
                        tracks.Add(currentTrack);
                    }
#pragma warning disable CS8620
                    tracks.AddRange(tracks_queue.Where(t => t != null));
#pragma warning restore CS8620

                    SqlServerWrapper.SaveTracks(tracks, Handler.GuildId);

                    Stop(source | CommandActionSource.Mute);

                    if (!mute)
                    {
                        Handler.Message.Send(new SaveException("Saved"), true);
                    }
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _ = _sqlSemaphore.Release();
            }
        }

        internal void Restore(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!_sqlSemaphore.WaitOne(1))
            {
                if (!mute)
                {
                    throw new RestoreException("Operation in progress");
                }
                return;
            }

            try
            {
                List<(ApiIntents, string)> info;
                try
                {
                    info = SqlServerWrapper.RestoreTracks(Handler.GuildId);
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(ex.GetExtendedMessage());
                    if (!mute)
                    {
                        throw new RestoreException("Restore failed", ex);
                    }
                    return;
                }
                if (info.Count == 0)
                {
                    if (!mute)
                    {
                        throw new RestoreException("Nothing to restore");
                    }
                    return;
                }
                foreach ((ApiIntents api, string id) in info)
                {
                    tracks_queue.Enqueue(ITrackInfo.GetTrack(api, id));
                    Handler.Log.Send("Track restored");
                }
                SqlServerWrapper.RemoveTracks(Handler.GuildId);
                if (!mute)
                {
                    Handler.Message.Send(new RestoreException("Restore success"), true);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                _ = _sqlSemaphore.Release();
            }
        }
    }
}
