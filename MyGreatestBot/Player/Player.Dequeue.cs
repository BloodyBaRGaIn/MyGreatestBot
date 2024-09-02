using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private void Dequeue()
        {
            while (true)
            {
                lock (queueLock)
                {
                    if (!tracksQueue.TryDequeue(out BaseTrackInfo? track))
                    {
                        lock (trackLock)
                        {
                            currentTrack = null;
                        }
                        break;
                    }

                    if (track == null)
                    {
                        continue;
                    }

                    {
                        DiscordEmbedBuilder builder;

                        builder = new PlayerException(track.GetMessage("Playing"))
                            .WithSuccess().GetDiscordEmbed();
                        builder.Thumbnail = track.Thumbnail;

                        Handler.Message.Send(builder);
                    }

                    if (track.Radio)
                    {
                        BaseTrackInfo? radio_track = null;
                        Exception? last_exception = null;

                        try
                        {
                            radio_track = ApiManager.GetRadio(track.TrackType, track.Id);
                        }
                        catch (Exception ex)
                        {
                            last_exception = ex;
                        }

                        if (radio_track == null)
                        {
                            track.Radio = false;
                            Handler.Message.Send(
                                new PlayerException(
                                    string.Join(Environment.NewLine,
                                        $"Cannot get the next radio track",
                                        last_exception)));
                        }
                        else
                        {
                            tracksQueue.EnqueueToHead(radio_track);
                            Handler.Log.Send(radio_track.GetMessage("Added radio", shortMessage: true));
                        }
                    }

                    ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance();

                    if (DbInstance != null && !track.BypassCheck)
                    {
                        DiscordEmbedBuilder? builder = null;

                        if (DbSemaphore.TryWaitOne())
                        {

                            try
                            {
                                if (DbInstance.IsAnyArtistIgnored(track, Handler.GuildId))
                                {
                                    builder = new DbIgnoreCommandException("Skipping track with ignored artist(s)")
                                        .WithSuccess().GetDiscordEmbed();
                                }
                                else if (DbInstance.IsTrackIgnored(track, Handler.GuildId))
                                {
                                    builder = new DbIgnoreCommandException("Skipping ignored track")
                                        .WithSuccess().GetDiscordEmbed();
                                }
                            }
                            catch (Exception ex)
                            {
                                builder = new DbIgnoreCommandException("Failed to check track", ex)
                                    .WithSuccess().GetDiscordEmbed();
                            }
                            finally
                            {
                                _ = DbSemaphore.TryRelease();
                            }
                        }

                        if (builder != null)
                        {
                            Handler.Message.Send(builder);
                            continue;
                        }
                    }

                    if (track.Duration >= MaxTrackDuration)
                    {
                        Handler.Message.Send(new DbIgnoreCommandException("Track is too long"));
                        continue;
                    }

                    if (!track.IsLiveStream && track.Duration <= MinTrackDuration)
                    {
                        Handler.Message.Send(new DbIgnoreCommandException("Track is too short"));
                        continue;
                    }

                    lock (trackLock)
                    {
                        currentTrack = track;
                    }

                    break;
                }
            }
        }
    }
}
