using DicordNET.ApiClasses.Spotify;
using DicordNET.ApiClasses.Vk;
using DicordNET.ApiClasses.Yandex;
using DicordNET.ApiClasses.Youtube;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace DicordNET.ApiClasses
{
    internal static class ApiConfig
    {
        private static ApiIntents InitIntents;

        internal static void InitApis(ApiIntents intents = ApiIntents.All)
        {
            InitIntents = intents;

            if ((InitIntents & ApiIntents.Spotify) != 0)
            {
                // init Yandex API for searching tracks from Spotify
                InitIntents |= ApiIntents.Yandex;
            }

            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                try
                {
                    YoutubeApiWrapper.PerformAuth();
                }
                catch
                {
                    throw new ApplicationException("Youtube auth failed");
                }
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                try
                {
                    YandexApiWrapper.PerformAuth();
                }
                catch
                {
                    throw new ApplicationException("Yandex auth failed");
                }
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                try
                {
                    VkApiWrapper.PerformAuth();
                }
                catch
                {
                    throw new ApplicationException("Vk auth failed");
                }
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                try
                {
                    SpotifyApiWrapper.PerformAuth();
                }
                catch
                {
                    throw new ApplicationException("Spotify auth failed");
                }
            }
        }

        internal static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube
                && (intents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                YoutubeApiWrapper.Logout();
                YoutubeApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex
                && (intents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                YandexApiWrapper.Logout();
                YandexApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk
                && (intents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                VkApiWrapper.Logout();
                VkApiWrapper.PerformAuth();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify
                && (intents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                SpotifyApiWrapper.Logout();
                SpotifyApiWrapper.PerformAuth();
            }
        }

        internal static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube
                && (intents & ApiIntents.Youtube) == ApiIntents.Youtube)
            {
                YoutubeApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex
                && (intents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                YandexApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk
                && (intents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                VkApiWrapper.Logout();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify
                && (intents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                SpotifyApiWrapper.Logout();
            }
        }

        private static class QueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex YOUTUBE_RE = new("^((http([s])?://)?((www|m)\\.)?youtube\\.([\\w])+/)");
            private static readonly Regex YANDEX_RE = new("^((http([s])?://)?music\\.yandex\\.([\\w])+/)");
            private static readonly Regex VK_RE = new("^((http([s])?://)?((www|m)\\.)?vk\\.com/)");
            private static readonly Regex SPOTIFY_RE = new("^((http([s])?://)?open\\.spotify\\.com/)");
#pragma warning restore SYSLIB1045

            internal static bool IsYoutubeURL([NotNull] string url)
            {
                return YOUTUBE_RE.IsMatch(url);
            }

            internal static bool IsYandexURL([NotNull] string url)
            {
                return YANDEX_RE.IsMatch(url);
            }

            internal static bool IsVkURL([NotNull] string url)
            {
                return VK_RE.IsMatch(url);
            }

            internal static bool IsSpotifyURL([NotNull] string url)
            {
                return SPOTIFY_RE.IsMatch(url);
            }
        }

        internal static List<ITrackInfo> GetAll(string? query)
        {
            List<ITrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query), "Invalid query");
            }

            if (QueryDecomposer.IsYoutubeURL(query))
            {
                if ((InitIntents & ApiIntents.Youtube) == ApiIntents.Youtube)
                {
                    tracks.AddRange(YoutubeApiWrapper.GetTracks(query));
                }
                else
                {
                    throw new InvalidOperationException($"{ApiIntents.Youtube} API not started");
                }
            }
            else if (QueryDecomposer.IsYandexURL(query))
            {
                if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex)
                {
                    tracks.AddRange(YandexApiWrapper.GetTracks(query));
                }
                else
                {
                    throw new InvalidOperationException($"{ApiIntents.Yandex} API not started");
                }
            }
            else if (QueryDecomposer.IsVkURL(query))
            {
                if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk)
                {
                    tracks.AddRange(VkApiWrapper.GetTracks(query));
                }
                else
                {
                    throw new InvalidOperationException($"{ApiIntents.Vk} API not started");
                }
            }
            else if (QueryDecomposer.IsSpotifyURL(query))
            {
                if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify)
                {
                    tracks.AddRange(SpotifyApiWrapper.GetTracks(query));
                }
                else
                {
                    throw new InvalidOperationException($"{ApiIntents.Spotify} API not started");
                }
            }
            else
            {
                throw new InvalidOperationException("Unknown query type");
            }

            return tracks;
        }
    }
}
