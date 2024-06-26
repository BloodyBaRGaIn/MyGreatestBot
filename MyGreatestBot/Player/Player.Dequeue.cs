﻿using DSharpPlus.Entities;
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
                    if (!tracksQueue.TryDequeue(out ITrackInfo? track))
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

                    DiscordEmbedBuilder builder;

                    builder = new PlayerException(track.GetMessage("Playing")).WithSuccess().GetDiscordEmbed();
                    builder.Thumbnail = track.GetThumbnail();

                    Handler.Message.Send(builder);

                    if (track.Radio)
                    {
                        ITrackInfo? radio_track = null;
                        string message = string.Empty;

                        try
                        {
                            radio_track = ApiManager.GetRadio(track.TrackType, track.Id);
                        }
                        catch (Exception ex)
                        {
                            message = ex.Message;
                        }

                        if (radio_track == null)
                        {
                            track.Radio = false;
                            Handler.Message.Send(
                                new PlayerException(
                                    string.Join(Environment.NewLine,
                                        $"Cannot get the next radio track",
                                        message)));
                        }
                        else
                        {
                            tracksQueue.EnqueueToHead(radio_track);
                            Handler.Log.Send(radio_track.GetShortMessage("Added radio"));
                        }
                    }

                    ITrackDatabaseAPI? DbInstance = ApiManager.GetDbApiInstance();

                    if (DbInstance != null && !track.BypassCheck)
                    {
                        bool semaphoerReady = true;

                        if (!DbSemaphore.TryWaitOne())
                        {
                            semaphoerReady = false;
                        }

                        if (semaphoerReady)
                        {
                            try
                            {
                                if (DbInstance.IsAnyArtistIgnored(track, Handler.GuildId))
                                {
                                    Handler.Message.Send(
                                        new DbIgnoreException("Skipping track with ignored artist(s)")
                                        .WithSuccess());

                                    continue;
                                }

                                if (DbInstance.IsTrackIgnored(track, Handler.GuildId))
                                {
                                    Handler.Message.Send(
                                        new DbIgnoreException("Skipping ignored track")
                                        .WithSuccess());

                                    continue;
                                }
                            }
                            catch (Exception ex)
                            {
                                Handler.Message.Send(
                                    new DbIgnoreException("Failed to check track", ex)
                                    .WithSuccess());

                                continue;
                            }
                            finally
                            {
                                _ = DbSemaphore.TryRelease();
                            }
                        }
                    }

                    if (track.Duration >= MaxTrackDuration)
                    {
                        Handler.Message.Send(new DbIgnoreException("Track is too long"));
                        continue;
                    }

                    if (!track.IsLiveStream && track.Duration <= MinTrackDuration)
                    {
                        Handler.Message.Send(new DbIgnoreException("Track is too short"));
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
