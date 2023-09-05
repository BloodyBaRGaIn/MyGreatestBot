using MyGreatestBot.ApiClasses;
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

#pragma warning disable CS8620
                List<ITrackInfo> tracks = new(tracks_queue.Where(t => t != null));
#pragma warning restore CS8620

                SqlServerWrapper.SaveTracks(tracks, Handler.GuildId);

                Clear(source | CommandActionSource.Mute);

                if (!mute)
                {
                    Handler.Message.Send(new SaveException("Saved"), true);
                }
            }
        }

        internal void Restore(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
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
                tracks_queue.Enqueue(GenericTrackInfo.GetTrack(api, id));
                Handler.Log.Send("Track restored");
            }
            SqlServerWrapper.RemoveTracks(Handler.GuildId);
            if (!mute)
            {
                Handler.Message.Send(new RestoreException("Restore success"), true);
            }
        }
    }
}
