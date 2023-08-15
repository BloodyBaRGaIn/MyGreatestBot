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
        /// <summary>
        /// Intents used on initialization
        /// </summary>
        private static ApiIntents InitIntents;

        /// <summary>
        /// <para>Performs auth for APIs</para>
        /// <para>Throws an exception if API auth process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for initialization</param>
        /// <exception cref="ApplicationException"></exception>
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
                    Console.WriteLine($"{ApiIntents.Youtube} SUCCESS");
                }
                catch
                {
                    Console.WriteLine($"{ApiIntents.Youtube} FAILED");
                    throw new ApplicationException($"{ApiIntents.Youtube} auth failed");
                }

                Task.Delay(1000).Wait();
            }

            if ((InitIntents & ApiIntents.Yandex) == ApiIntents.Yandex)
            {
                try
                {
                    YandexApiWrapper.PerformAuth();
                    Console.WriteLine($"{ApiIntents.Yandex} SUCCESS");
                }
                catch
                {
                    Console.WriteLine($"{ApiIntents.Yandex} FAILED");
                    throw new ApplicationException($"{ApiIntents.Yandex} auth failed");
                }

                Task.Delay(1000).Wait();
            }

            if ((InitIntents & ApiIntents.Vk) == ApiIntents.Vk)
            {
                try
                {
                    VkApiWrapper.PerformAuth();
                    Console.WriteLine($"{ApiIntents.Vk} SUCCESS");
                }
                catch
                {
                    Console.WriteLine($"{ApiIntents.Vk} FAILED");
                    throw new ApplicationException($"{ApiIntents.Vk} auth failed");
                }

                Task.Delay(1000).Wait();
            }

            if ((InitIntents & ApiIntents.Spotify) == ApiIntents.Spotify)
            {
                try
                {
                    SpotifyApiWrapper.PerformAuth();
                    Console.WriteLine($"{ApiIntents.Spotify} SUCCESS");
                }
                catch
                {
                    Console.WriteLine($"{ApiIntents.Spotify} FAILED");
                    throw new ApplicationException($"{ApiIntents.Spotify} auth failed");
                }

                Task.Delay(1000).Wait();
            }
        }

        /// <summary>
        /// <para>Performs re-auth for APIs</para>
        /// <para>Throws an exception if API re-auth process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for reinitialization</param>
        /// <exception cref="ApplicationException"></exception>
        internal static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            try
            {
                if ((intents & ApiIntents.Youtube) == ApiIntents.Youtube)
                {
                    YoutubeApiWrapper.Logout();
                    Task.Delay(500).Wait();
                    YoutubeApiWrapper.PerformAuth();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Yandex) == ApiIntents.Yandex)
                {
                    YandexApiWrapper.Logout();
                    Task.Delay(500).Wait();
                    YandexApiWrapper.PerformAuth();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Vk) == ApiIntents.Vk)
                {
                    VkApiWrapper.Logout();
                    Task.Delay(500).Wait();
                    VkApiWrapper.PerformAuth();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Spotify) == ApiIntents.Spotify)
                {
                    SpotifyApiWrapper.Logout();
                    Task.Delay(500).Wait();
                    SpotifyApiWrapper.PerformAuth();
                    Task.Delay(500).Wait();
                }
            }
            catch
            {
                throw new ApplicationException();
            }
        }

        /// <summary>
        /// <para>Performs logout</para>
        /// <para>Throws an exception if API logout process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for logout</param>
        /// <exception cref="ApplicationException"></exception>
        internal static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            try
            {
                if ((intents & ApiIntents.Youtube) == ApiIntents.Youtube)
                {
                    YoutubeApiWrapper.Logout();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Yandex) == ApiIntents.Yandex)
                {
                    YandexApiWrapper.Logout();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Vk) == ApiIntents.Vk)
                {
                    VkApiWrapper.Logout();
                    Task.Delay(500).Wait();
                }

                if ((intents & ApiIntents.Spotify) == ApiIntents.Spotify)
                {
                    SpotifyApiWrapper.Logout();
                    Task.Delay(500).Wait();
                }
            }
            catch
            {
                throw new ApplicationException();
            }
        }

        /// <summary>
        /// Links identifier class
        /// </summary>
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

        /// <summary>
        /// Get all tracks from specified URL
        /// </summary>
        /// <param name="query">URL</param>
        /// <returns>List of tracks</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
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
