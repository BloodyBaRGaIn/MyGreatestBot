using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyGreatestBot.ApiClasses.Music
{
    public static partial class QueryIdentifier
    {
        // Youtube link regular expression
        private static readonly Regex YoutubeRegex = GenerateYoutubeRegex();
        // Youtube reduced link regular expression
        private static readonly Regex YoutubeReducedRegex = GenerateToutubeReducedRegex();
        // Yandex link regular expression
        private static readonly Regex YandexRegex = GenerateYandexRegex();
        // Vk link regular expression
        private static readonly Regex VkRegex = GenerateVkRegex();
        // Spotify link regular expression
        private static readonly Regex SpotifyRegex = GenerateSpotifyRegex();
        // generic search query
        private static readonly Regex GenericRegex = GenerateCommonRegex();

        public static IEnumerable<BaseTrackInfo>? GetTracks(string query)
        {
            return TracksReceiver.Execute(query);
        }

        public static BaseTrackInfo? GetRadio(ApiIntents intents, string id)
        {
            return TracksRadio.Execute(intents, id);
        }

        private sealed class TracksRadio
        {
            private delegate BaseTrackInfo? GetRadio(string id);

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

            internal static BaseTrackInfo? Execute(ApiIntents intents, string id)
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
            private delegate IEnumerable<BaseTrackInfo>? GetTracks(string query);

            private readonly Regex[] patterns;
            private readonly IMusicAPI api;
            private readonly GetTracks getTracks;

            private TracksReceiver(IMusicAPI api, GetTracks getTracks, params Regex[] patterns)
            {
                this.patterns = patterns;
                this.api = api;
                this.getTracks = getTracks;
            }

            internal static IEnumerable<BaseTrackInfo>? Execute(string query)
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
                    YoutubeRegex, YoutubeReducedRegex),

                new TracksReceiver(
                    YandexApiWrapper.UrlMusicInstance,
                    YandexApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    YandexRegex),

                new TracksReceiver(
                    VkApiWrapper.UrlMusicInstance,
                    VkApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    VkRegex),

                new TracksReceiver(
                    SpotifyApiWrapper.UrlMusicInstance,
                    SpotifyApiWrapper.UrlMusicInstance.GetTracksFromUrl,
                    SpotifyRegex),

                new TracksReceiver(
                    YoutubeApiWrapper.TextMusicInstance,
                    YoutubeApiWrapper.TextMusicInstance.GetTracksFromPlainText,
                    GenericRegex)
            ];
        }

        [GeneratedRegex("^((http([s])?://)?((www|m)\\.)?(youtube\\.([\\w])+)/)")]
        private static partial Regex GenerateYoutubeRegex();

        [GeneratedRegex("^((http([s])?://)?((www|m)\\.)?youtu\\.be/)")]
        private static partial Regex GenerateToutubeReducedRegex();

        [GeneratedRegex("^((http([s])?://)?music\\.yandex\\.([\\w])+/)")]
        private static partial Regex GenerateYandexRegex();

        [GeneratedRegex("^((http([s])?://)?((www|m)\\.)?vk\\.com/)")]
        private static partial Regex GenerateVkRegex();

        [GeneratedRegex("^((http([s])?://)?open\\.spotify\\.com/)")]
        private static partial Regex GenerateSpotifyRegex();

        [GeneratedRegex("((\\s)|(\\S))+")]
        private static partial Regex GenerateCommonRegex();
    }
}
