using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void SqlRestore(CommandActionSource source)
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
                    throw new SqlRestoreException("Operation in progress");
                }
                return;
            }

            try
            {
                List<(ApiIntents, string)> info;
                try
                {
                    info = SqlServerWrapper.Instance.RestoreTracks(Handler.GuildId);
                }
                catch (Exception ex)
                {
                    DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
                    if (!mute)
                    {
                        throw new SqlRestoreException("Restore failed", ex);
                    }
                    return;
                }
                if (info.Count == 0)
                {
                    if (!mute)
                    {
                        throw new SqlRestoreException("Nothing to restore");
                    }
                    return;
                }
                int restoreCount = 0;
                foreach ((ApiIntents api, string id) in info)
                {
                    ITrackInfo? track = ITrackInfo.GetTrack(api, id);
                    if (track == null)
                    {
                        continue;
                    }
                    tracks_queue.Enqueue(track);
                    Handler.Log.Send(track.GetShortMessage("Track restored: "));
                    restoreCount++;
                }
                SqlServerWrapper.Instance.RemoveTracks(Handler.GuildId);
                if (!mute)
                {
                    Handler.Message.Send(new SqlRestoreException($"Restored {restoreCount} track(s)"), true);
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
