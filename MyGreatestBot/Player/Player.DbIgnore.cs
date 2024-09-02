using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.ApiClasses.Services.Discord.Handlers;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void DbIgnoreTrack(CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            lock (trackLock)
            {
                if (!IsPlaying || currentTrack == null)
                {
                    messageHandler?.Send(new DbIgnoreCommandException("Nothing to ignore"));
                    return;
                }

                if (currentTrack.BypassCheck)
                {
                    messageHandler?.Send(new DbIgnoreCommandException("Cannot ignore track in bypass mode"));
                    return;
                }

                try
                {
                    DbInstance.AddIgnoredTrack(currentTrack, Handler.GuildId);
                }
                catch (Exception ex)
                {
                    messageHandler?.Send(new DbIgnoreCommandException("Failed to ignore track. Skipping", ex));
                    return;
                }
                finally
                {
                    IsPlaying = false;
                    WaitForFinish();
                }

                messageHandler?.Send(new DbIgnoreCommandException("Track ignored").WithSuccess());
            }
        }

        internal void DbIgnoreArtist(int index, CommandActionSource source)
        {
            MessageHandler? messageHandler = source.HasFlag(CommandActionSource.Mute)
                ? null
                : Handler.Message;

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            lock (trackLock)
            {
                if (!IsPlaying || currentTrack == null)
                {
                    messageHandler?.Send(new DbIgnoreCommandException("Nothing to ignore"));
                    return;
                }

                if (currentTrack.BypassCheck)
                {
                    messageHandler?.Send(new DbIgnoreCommandException("Cannot ignore artist(s) in bypass mode"));
                    return;
                }

                int start, max;

                if (index < 0)
                {
                    start = 0;
                    max = currentTrack.ArtistArr.Length;
                }
                else
                {
                    start = index;
                    max = index;
                }

                Exception? last_exception = null;

                for (int i = start; i < max; i++)
                {
                    try
                    {
                        DbInstance.AddIgnoredArtist(currentTrack, Handler.GuildId, i);
                    }
                    catch (Exception ex)
                    {
                        last_exception = ex;
                    }
                }

                IsPlaying = false;
                WaitForFinish();

                messageHandler?.Send(last_exception != null
                    ? new DbIgnoreCommandException("Failed to ignore artist", last_exception)
                    : new DbIgnoreCommandException("Artist(s) ignored").WithSuccess());
            }
        }
    }
}
