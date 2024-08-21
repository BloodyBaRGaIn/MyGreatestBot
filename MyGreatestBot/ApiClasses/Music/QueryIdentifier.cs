using System;
using System.Collections.Generic;
using System.Linq;
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
            private readonly IMusicAPI api;

            private TracksRadio(IMusicAPI api)
            {
                this.api = api;
                get_radio = api switch
                {
                    IRadioMusicAPI radio => radio.GetRadio,
                    _ => throw new ArgumentException("Invalid API instance")
                };
            }

            internal static ITrackInfo? Execute(ApiIntents intents, string id)
            {
                foreach (TracksRadio radio in collection)
                {
                    if ((radio.api.ApiType & intents) != ApiIntents.None)
                    {
                        return ApiManager.IsApiRegisterdAndAllowed(radio.api.ApiType)
                            ? radio.get_radio.Invoke(id)
                            : throw new ApiException(radio.api.ApiType);
                    }
                }

                throw new InvalidOperationException("Radio mode is not supported");
            }

            private static readonly TracksRadio[] collection =
            [
                new TracksRadio(YandexApiWrapper.RadioMusicInstance),
            ];
        }

        private sealed class TracksReceiver
        {
            private delegate IEnumerable<ITrackInfo>? GetTracks(string query);

            private readonly Regex[] patterns;
            private readonly IMusicAPI api;
            private readonly GetTracks getTracks;

            private TracksReceiver(IMusicAPI api, GetTracks getTracks, params Regex[] patterns)
            {
                this.patterns = patterns;
                this.api = api;
                this.getTracks = getTracks;
            }

            internal static IEnumerable<ITrackInfo>? Execute(string query)
            {
                foreach (TracksReceiver receiver in collection)
                {
                    if (receiver.patterns.Any(p => p.IsMatch(query)))
                    {
                        return ApiManager.IsApiRegisterdAndAllowed(receiver.api.ApiType)
                            ? receiver.getTracks.Invoke(query)
                            : throw new ApiException(receiver.api.ApiType);
                    }
                }

                throw new InvalidOperationException("Unknown music format");
            }

            private static readonly TracksReceiver[] collection =
            [
                new TracksReceiver(
                    YoutubeApiWrapper.UrlMusicInstance,
                    YoutubeApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    YOUTUBE_RE, YOUTUBE_REDUCED_RE),

                new TracksReceiver(
                    YandexApiWrapper.UrlMusicInstance,
                    YandexApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    YANDEX_RE),

                new TracksReceiver(
                    VkApiWrapper.UrlMusicInstance,
                    VkApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    VK_RE),

                new TracksReceiver(
                    SpotifyApiWrapper.UrlMusicInstance,
                    SpotifyApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    SPOTIFY_RE),

                new TracksReceiver(
                    YoutubeApiWrapper.TextMusicInstance,
                    YoutubeApiWrapper.TextMusicInstance.GetTracksFromPlainText,
                    GENERIC_RE)
            ];
        }
    }
}
