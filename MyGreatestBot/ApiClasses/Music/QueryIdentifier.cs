using System;
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
        private static readonly Regex YOUTUBE_REDUCED_RE = new("^((http([s])?://)?((www|m)\\.)?youtu\\.be/)");
        private static readonly Regex YANDEX_RE = new("^((http([s])?://)?music\\.yandex\\.([\\w])+/)");
        private static readonly Regex VK_RE = new("^((http([s])?://)?((www|m)\\.)?vk\\.com/)");
        private static readonly Regex SPOTIFY_RE = new("^((http([s])?://)?open\\.spotify\\.com/)");
        // generic search query
        private static readonly Regex GENERIC_RE = new("((\\s)|(\\S))+");
#pragma warning restore SYSLIB1045

        public static IEnumerable<ITrackInfo>? GetTracks(string query)
        {
            return TracksReceiver.Execute(query);
        }

        public static ITrackInfo? GetRadio(ApiIntents intents, string id)
        {
            return TracksRadio.Execute(intents, id);
        }

        private sealed class TracksRadio
        {
            private delegate ITrackInfo? GetRadio(string id);

            private readonly GetRadio get_radio;
            private readonly ApiIntents desired;

            private TracksRadio(ApiIntents desired, GetRadio get_radio)
            {
                this.desired = desired;
                this.get_radio = get_radio;
            }

            internal static ITrackInfo? Execute(ApiIntents intents, string id)
            {
                foreach (TracksRadio radio in collection)
                {
                    if ((radio.desired & intents) == ApiIntents.None)
                    {
                        continue;
                    }
                    return ApiManager.IsApiRegisterdAndAllowed(radio.desired)
                        ? radio.get_radio.Invoke(id)
                        : throw new ApiException(radio.desired);
                }

                throw new InvalidOperationException("Radio mode is not supported");
            }

            private static readonly TracksRadio[] collection =
            [
                //new(ApiIntents.Youtube, Youtube.YoutubeApiWrapper.Instance.GetRadio),
                new(ApiIntents.Yandex, Yandex.YandexApiWrapper.RadioInstance.GetRadio),
                //new(ApiIntents.Vk, Vk.VkApiWrapper.Instance.GetRadio),
                //new(ApiIntents.Spotify, Spotify.SpotifyApiWrapper.Instance.GetRadio)
            ];
        }

        private sealed class TracksReceiver
        {
            private delegate IEnumerable<ITrackInfo>? GetTracks(string query);

            private readonly Regex pattern;
            private readonly IAPI api;
            private readonly GetTracks getTracks;

            private TracksReceiver(Regex pattern, IAPI api)
            {
                this.pattern = pattern;
                this.api = api;
                this.getTracks = api switch
                {
                    IQueryMusicAPI query => query.GetTracksFromPlainText,
                    IMusicAPI music => music.GetTracks,
                    _ => throw new ArgumentException("Invalid API instance"),
                };
            }

            internal static IEnumerable<ITrackInfo>? Execute(string query)
            {
                foreach (TracksReceiver receiver in collection)
                {
                    if (!receiver.pattern.IsMatch(query))
                    {
                        continue;
                    }
                    return ApiManager.IsApiRegisterdAndAllowed(receiver.api.ApiType)
                        ? receiver.getTracks.Invoke(query)
                        : throw new ApiException(receiver.api.ApiType);
                }

                throw new InvalidOperationException("Unknown music format");
            }

            private static readonly TracksReceiver[] collection =
            [
                new TracksReceiver(YOUTUBE_RE, YoutubeApiWrapper.MusicInstance),
                new TracksReceiver(YOUTUBE_REDUCED_RE, YoutubeApiWrapper.MusicInstance),
                new TracksReceiver(YANDEX_RE, YandexApiWrapper.MusicInstance),
                new TracksReceiver(VK_RE, VkApiWrapper.MusicInstance),
                new TracksReceiver(SPOTIFY_RE, SpotifyApiWrapper.MusicInstance),
                new TracksReceiver(GENERIC_RE, YoutubeApiWrapper.QueryInstance)
            ];
        }
    }
}
