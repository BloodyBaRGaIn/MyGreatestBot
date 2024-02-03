﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyGreatestBot.ApiClasses.Music
{
    public static class QueryIdentifier
    {
#pragma warning disable SYSLIB1045
        // regular Youtube link
        private static readonly Regex YOUTUBE_RE = new("^((http([s])?://)?((www|m)\\.)?(youtube\\.([\\w])+)/)");
        // reduced Youtube link
        private static readonly Regex YOUTUBE_SHORT_RE = new("^((http([s])?://)?((www|m)\\.)?youtu\\.be/)");
        private static readonly Regex YANDEX_RE = new("^((http([s])?://)?music\\.yandex\\.([\\w])+/)");
        private static readonly Regex VK_RE = new("^((http([s])?://)?((www|m)\\.)?vk\\.com/)");
        private static readonly Regex SPOTIFY_RE = new("^((http([s])?://)?open\\.spotify\\.com/)");
        // generic search query
        private static readonly Regex GENERIC_RE = new("((\\s)|(\\S))+");
#pragma warning restore SYSLIB1045

        public static IEnumerable<ITrackInfo>? Execute(string query)
        {
            return TracksReceiver.Execute(query);
        }

        private sealed class TracksReceiver
        {
            private delegate IEnumerable<ITrackInfo>? GetTracks(string query);

            private readonly Regex pattern;
            private readonly GetTracks get_tracks;
            private readonly ApiIntents desired;

            private TracksReceiver(ApiIntents desired, Regex pattern, GetTracks get_tracks)
            {
                this.desired = desired;
                this.pattern = pattern;
                this.get_tracks = get_tracks;
            }

            internal static IEnumerable<ITrackInfo>? Execute(string query)
            {
                foreach (TracksReceiver receiver in collection)
                {
                    if (!receiver.pattern.IsMatch(query))
                    {
                        continue;
                    }
                    return ApiManager.InitIntents.HasFlag(receiver.desired)
                        ? receiver.get_tracks.Invoke(query)
                        : throw new ApiException(receiver.desired);
                }

                throw new InvalidOperationException("Unknown music format");
            }

            private static readonly TracksReceiver[] collection =
            [
                new(ApiIntents.Youtube, YOUTUBE_RE, Youtube.YoutubeApiWrapper.Instance.GetTracks),
                new(ApiIntents.Youtube, YOUTUBE_SHORT_RE, Youtube.YoutubeApiWrapper.Instance.GetTracks),
                new(ApiIntents.Yandex, YANDEX_RE, Yandex.YandexApiWrapper.Instance.GetTracks),
                new(ApiIntents.Vk, VK_RE, Vk.VkApiWrapper.Instance.GetTracks),
                new(ApiIntents.Spotify, SPOTIFY_RE, Spotify.SpotifyApiWrapper.Instance.GetTracks),
                new(ApiIntents.Youtube, GENERIC_RE, Youtube.YoutubeApiWrapper.Instance.GetTracksSearch)
            ];
        }
    }
}
