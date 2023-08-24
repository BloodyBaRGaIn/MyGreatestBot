using DicordNET.ApiClasses.Spotify;
using DicordNET.ApiClasses.Vk;
using DicordNET.ApiClasses.Yandex;
using DicordNET.ApiClasses.Youtube;
using DicordNET.Sql;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;

namespace DicordNET.ApiClasses
{
    [SupportedOSPlatform("windows")]
    internal static class ApiConfig
    {
        /// <summary>
        /// Intents used on initialization
        /// </summary>
        internal static ApiIntents InitIntents { get; private set; }

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

            Init(ApiIntents.Sql, SqlServerWrapper.Open);
            Init(ApiIntents.Youtube, YoutubeApiWrapper.PerformAuth);
            Init(ApiIntents.Yandex, YandexApiWrapper.PerformAuth);
            Init(ApiIntents.Yandex, YandexApiWrapper.PerformAuth);
            Init(ApiIntents.Vk, VkApiWrapper.PerformAuth);
            Init(ApiIntents.Spotify, SpotifyApiWrapper.PerformAuth);
        }

        private static void Init(ApiIntents desired, Action init_action, int delay = 500)
        {
            if (init_action is null || !InitIntents.HasFlag(desired))
            {
                return;
            }

            try
            {
                init_action.Invoke();
                Console.WriteLine($"{desired} SUCCESS");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{desired} FAILED");
                throw new ApplicationException($"{desired} auth failed", ex);
            }
            finally
            {
                Task.Delay(delay).Wait();
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

            Reload(intents, ApiIntents.Sql, SqlServerWrapper.Open, SqlServerWrapper.Close);
            Reload(intents, ApiIntents.Youtube, YoutubeApiWrapper.PerformAuth);
            Reload(intents, ApiIntents.Yandex, YandexApiWrapper.PerformAuth);
            Reload(intents, ApiIntents.Vk, VkApiWrapper.PerformAuth, VkApiWrapper.Logout);
            Reload(intents, ApiIntents.Spotify, SpotifyApiWrapper.PerformAuth);
        }

        private static void Reload(ApiIntents allowed, ApiIntents desied, Action init_action, Action? deinit_action = null, int delay = 500)
        {
            Deinit(allowed, desied, deinit_action, delay);
            Init(desied, init_action, delay);
        }

        /// <summary>
        /// <para>Performs logout</para>
        /// <para>Throws an exception if API logout process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for logout</param>
        internal static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            Deinit(intents, ApiIntents.Sql, SqlServerWrapper.Close);
            Deinit(intents, ApiIntents.Youtube);
            Deinit(intents, ApiIntents.Yandex);
            Deinit(intents, ApiIntents.Vk, VkApiWrapper.Logout);
            Deinit(intents, ApiIntents.Spotify);
        }

        private static void Deinit(ApiIntents allowed, ApiIntents desied, Action? deinit_action = null, int delay = 500)
        {
            if (deinit_action is null || !allowed.HasFlag(desied))
            {
                return;
            }

            try
            {
                deinit_action.Invoke();
            }
            catch
            {
                ;
            }
            finally
            {
                Task.Delay(delay).Wait();
            }
        }

        /// <summary>
        /// Get all tracks from specified URL
        /// </summary>
        /// <param name="query">URL</param>
        /// <returns>List of tracks</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException"></exception>
        internal static IEnumerable<ITrackInfo> GetAll(string? query)
        {
            IEnumerable<ITrackInfo> tracks;

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query), "Invalid query");
            }

            tracks = QueryIdentifier.Execute(query);

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            return tracks;
        }
    }
}
