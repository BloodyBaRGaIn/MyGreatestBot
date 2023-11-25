using MyGreatestBot.ApiClasses;
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
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!ApiManager.InitIntents.HasFlag(ApiIntents.Sql))
            {
                throw new SqlApiException();
            }

            if (!_sqlSemaphore.WaitOne(1))
            {
                if (!mute)
                {
                    throw new SqlSaveException("Operation in progress");
                }
                return;
            }

            try
            {
                if (!tracks_queue.Any())
                {
                    if (!mute)
                    {
                        throw new SqlSaveException("Nothing to save");
                    }
                    return;
                }
                lock (tracks_queue)
                {
                    //SqlServerWrapper.Instance.RemoveTracks(Handler.GuildId);

                    List<ITrackInfo> tracks = new();
                    if (currentTrack != null)
                    {
                        tracks.Add(currentTrack);
                    }

#pragma warning disable CS8620
                    tracks.AddRange(tracks_queue.Where(t => t != null));
#pragma warning restore CS8620

                    SqlServerWrapper.Instance.SaveTracks(tracks, Handler.GuildId);

                    int tracksCount = tracks.Count;

                    Stop(source | CommandActionSource.Mute);

                    if (!mute)
                    {
                        Handler.Message.Send(new SqlSaveException($"Saved {tracksCount} track(s)"), true);
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
