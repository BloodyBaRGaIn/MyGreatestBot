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
        private static readonly Regex YOUTUBE_SHORT_RE = new("^((http([s])?://)?((www|m)\\.)?youtu\\.be/)");
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
                    return ApiManager.IsApiRegisterdAndAllowed(receiver.desired)
                        ? receiver.get_tracks.Invoke(query)
                        : throw new ApiException(receiver.desired);
                }

                throw new InvalidOperationException("Unknown music format");
            }

            private static readonly TracksReceiver[] collection =
            [
                new(ApiIntents.Youtube, YOUTUBE_RE, Youtube.YoutubeApiWrapper.MusicInstance.GetTracks),
                new(ApiIntents.Youtube, YOUTUBE_SHORT_RE, Youtube.YoutubeApiWrapper.MusicInstance.GetTracks),
                new(ApiIntents.Yandex, YANDEX_RE, Yandex.YandexApiWrapper.MusicInstance.GetTracks),
                new(ApiIntents.Vk, VK_RE, Vk.VkApiWrapper.MusicInstance.GetTracks),
                new(ApiIntents.Spotify, SPOTIFY_RE, Spotify.SpotifyApiWrapper.MusicInstance.GetTracks),
                new(ApiIntents.Youtube, GENERIC_RE, Youtube.YoutubeApiWrapper.MusicInstance.GetTracksFromPlainText)
            ];
        }
    }
}
