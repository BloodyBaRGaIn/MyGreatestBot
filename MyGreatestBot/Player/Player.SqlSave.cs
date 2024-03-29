﻿using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void SqlSave(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!_sqlSemaphore.WaitOne(1))
            {
                if (nomute)
                {
                    throw new SqlSaveException("Operation in progress");
                }
                return;
            }

            try
            {
                if (tracks_queue.Count == 0)
                {
                    if (nomute)
                    {
                        throw new SqlSaveException("Nothing to save");
                    }
                    return;
                }
                lock (tracks_queue)
                {
                    //SqlServerWrapper.Instance.RemoveTracks(Handler.GuildId);

                    List<ITrackInfo> tracks = currentTrack != null ? [currentTrack] : [];

#pragma warning disable CS8620
                    tracks.AddRange(tracks_queue.Where(t => t != null));
#pragma warning restore CS8620

                    SqlServerWrapper.Instance.SaveTracks(tracks, Handler.GuildId);

                    int tracksCount = tracks.Count;

                    Stop(source | CommandActionSource.Mute);

                    if (nomute)
                    {
                        Handler.Message.Send(new SqlSaveException($"Saved {tracksCount} track(s)").WithSuccess());
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

    }
}
