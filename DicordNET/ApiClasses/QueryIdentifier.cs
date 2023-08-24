using DicordNET.ApiClasses.Spotify;
using DicordNET.ApiClasses.Vk;
using DicordNET.ApiClasses.Yandex;
using DicordNET.ApiClasses.Youtube;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace DicordNET.ApiClasses
{
    /// <summary>
    /// Links identifier class
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class QueryIdentifier
    {
#pragma warning disable SYSLIB1045
        private static readonly Regex YOUTUBE_RE = new("^((http([s])?://)?((www|m)\\.)?youtube\\.([\\w])+/)");
        private static readonly Regex YANDEX_RE = new("^((http([s])?://)?music\\.yandex\\.([\\w])+/)");
        private static readonly Regex VK_RE = new("^((http([s])?://)?((www|m)\\.)?vk\\.com/)");
        private static readonly Regex SPOTIFY_RE = new("^((http([s])?://)?open\\.spotify\\.com/)");
#pragma warning restore SYSLIB1045

        internal static IEnumerable<ITrackInfo> Execute(string query) => TracksReceiver.Execute(query);

        private sealed class TracksReceiver
        {
            private delegate IEnumerable<ITrackInfo> GetTracks(string query);

            private readonly Regex pattern;
            private readonly GetTracks get_tracks;
            private readonly ApiIntents desired;

            private TracksReceiver(ApiIntents desired, Regex pattern, GetTracks get_tracks)
            {
                this.desired = desired;
                this.pattern = pattern;
                this.get_tracks = get_tracks;
            }

            internal static IEnumerable<ITrackInfo> Execute(string query)
            {
                foreach (TracksReceiver receiver in collection)
                {
                    if (!receiver.pattern.IsMatch(query))
                    {
                        continue;
                    }
                    if (ApiConfig.InitIntents.HasFlag(receiver.desired))
                    {
                        return receiver.get_tracks.Invoke(query);
                    }
                    else
                    {
                        throw new InvalidOperationException($"{receiver.desired} API not started");
                    }
                }

                throw new InvalidOperationException("Unknown query type");
            }

            private static readonly TracksReceiver[] collection = new TracksReceiver[]
            {
                new(ApiIntents.Youtube, YOUTUBE_RE, YoutubeApiWrapper.GetTracks),
                new(ApiIntents.Yandex, YANDEX_RE, YandexApiWrapper.GetTracks),
                new(ApiIntents.Vk, VK_RE, VkApiWrapper.GetTracks),
                new(ApiIntents.Spotify, SPOTIFY_RE, SpotifyApiWrapper.GetTracks),
            };
        }
    }
}
