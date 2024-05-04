using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void DbIgnoreTrack(CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            lock (trackLock)
            {
                if (!IsPlaying || currentTrack == null)
                {
                    if (nomute)
                    {
                        Handler.Message.Send(new DbIgnoreException("Nothing to ignore"));
                    }
                    return;
                }

                DiscordEmbedBuilder? builder = null;

                try
                {
                    DbInstance.AddIgnoredTrack(currentTrack, Handler.GuildId);
                }
                catch (Exception ex)
                {
                    if (nomute)
                    {
                        builder = new DbIgnoreException("Failed to ignore track", ex).GetDiscordEmbed();
                    }
                }

                IsPlaying = false;
                WaitForFinish();

                builder ??= new DbIgnoreException("Track ignored").WithSuccess().GetDiscordEmbed();

                if (nomute)
                {
                    Handler.Message.Send(builder);
                }
            }
        }

        internal void DbIgnoreArtist(int index, CommandActionSource source)
        {
            bool nomute = !source.HasFlag(CommandActionSource.Mute);

            ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance() ?? throw new DbApiException();

            lock (trackLock)
            {
                if (!IsPlaying || currentTrack == null)
                {
                    if (nomute)
                    {
                        Handler.Message.Send(new DbIgnoreException("Nothing to ignore"));
                    }
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

                for (int i = start; i < max; i++)
                {
                    try
                    {
                        DbInstance.AddIgnoredArtist(currentTrack, Handler.GuildId, i);
                    }
                    catch (Exception ex)
                    {
                        Handler.Message.Send(new DbIgnoreException("Failed to ignore artist", ex));
                    }
                }

                IsPlaying = false;
                WaitForFinish();

                if (nomute)
                {
                    Handler.Message.Send(new DbIgnoreException("Artist ignored").WithSuccess());
                }
            }
        }
    }
}
