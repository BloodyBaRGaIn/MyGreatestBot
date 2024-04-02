using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    [SupportedOSPlatform("windows")]
    internal sealed partial class Player
    {
        private void Dequeue()
        {
            while (true)
            {
                Task.Yield().GetAwaiter().GetResult();
                lock (tracks_queue)
                {
                    if (!tracks_queue.TryDequeue(out ITrackInfo? track))
                    {
                        currentTrack = null;
                        return;
                    }

                    if (track == null)
                    {
                        continue;
                    }

                    DiscordEmbedBuilder builder;

                    try
                    {
                        builder = GetPlayingMessage<PlayerException>(track, "Playing");
                    }
                    catch
                    {
                        builder = new PlayerException("Cannot make playing message").GetDiscordEmbed();
                    }

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
                                new PlayerException($"Cannot get the next radio track" +
                                $"{(message != string.Empty ? $"{Environment.NewLine}{message}" : "")}"));
                        }
                        else
                        {
                            tracks_queue.EnqueueToHead(radio_track);
                        }
                    }

                    if (!track.BypassCheck)
                    {
                        if (SqlServerWrapper.Instance.IsAnyArtistIgnored(track, Handler.GuildId))
                        {
                            Handler.Message.Send(new IgnoreException("Skipping track with ignored artist(s)").WithSuccess());
                            continue;
                        }

                        if (SqlServerWrapper.Instance.IsTrackIgnored(track, Handler.GuildId))
                        {
                            Handler.Message.Send(new IgnoreException("Skipping ignored track").WithSuccess());
                            continue;
                        }
                    }

                    if (track.Duration >= MaxTrackDuration)
                    {
                        Handler.Message.Send(new IgnoreException("Track is too long").WithSuccess());
                        continue;
                    }

                    if (!track.IsLiveStream && track.Duration <= MinTrackDuration)
                    {
                        Handler.Message.Send(new IgnoreException("Track is too short").WithSuccess());
                        continue;
                    }

                    currentTrack = track;
                    break;
                }
            }
        }
    }
}
