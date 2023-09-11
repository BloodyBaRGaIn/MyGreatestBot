using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API configuration
    /// </summary>
    public static class ApiManager
    {
        /// <summary>
        /// Intents used on initialization
        /// </summary>
        public static ApiIntents InitIntents { get; private set; } = ApiIntents.None;

        public static ApiIntents FailedIntents { get; private set; } = ApiIntents.None;

        private static readonly Dictionary<ApiIntents, IAPI?> ApiCollection = new();

        public static void Add([DisallowNull] IAPI api)
        {
            ApiCollection.Add(api.ApiType, api);
        }

        public static T? Get<T>(ApiIntents intents) where T : IAPI
        {
            return (T?)ApiCollection[intents];
        }

        public static void TryAcessDomain(ApiIntents intents)
        {
            if (ApiCollection.TryGetValue(intents, out IAPI? api))
            {
                if (api is IAccessible accessible)
                {
                    accessible.TryAcess();
                }
            }
        }

        public static void InitApis(ApiIntents intents = ApiIntents.All)
        {
            InitIntents = intents;

            if (InitIntents.HasFlag(ApiIntents.Spotify))
            {
                // init Yandex API for searching tracks from Spotify
                InitIntents |= ApiIntents.Yandex;
            }

            if (InitIntents.HasFlag(ApiIntents.Youtube))
            {
                // YoutubeExplode won't work without specific environment variable
                Utils.YoutubeExplodeBypass.Bypass();
            }

            foreach (IAPI? api in ApiCollection.Values)
            {
                if (api != null)
                {
                    Init(api);
                }
            }
        }

        private static void Init(IAPI desired, int delay = 500)
        {
            if (!InitIntents.HasFlag(desired.ApiType))
            {
                return;
            }

            InitBody(desired, delay);
        }

        private static void InitBody(IAPI desired, int delay)
        {
            try
            {
                if (desired is IAccessible accessible)
                {
                    accessible.TryAcess();
                }

                desired.PerformAuth();
                Console.WriteLine($"{desired.ApiType} SUCCESS");
                FailedIntents &= ~desired.ApiType;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{desired.ApiType} FAILED");
                Console.Error.WriteLine(ex.GetExtendedMessage());
                try
                {
                    desired.Logout();
                }
                catch { }
                FailedIntents |= desired.ApiType;
            }
            finally
            {
                Task.Delay(delay).Wait();
            }
        }

        public static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            foreach (IAPI? api in ApiCollection.Values)
            {
                if (api != null)
                {
                    Deinit(intents, api);
                }
            }
        }

        private static void Deinit(ApiIntents allowed, IAPI desired, int delay = 500)
        {
            if (allowed.HasFlag(desired.ApiType))
            {
                DeinitBody(desired, delay);
            }
        }

        private static void DeinitBody(IAPI desired, int delay)
        {
            try
            {
                desired.Logout();
            }
            catch { }
            finally
            {
                Task.Delay(delay).Wait();
            }
        }

        public static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            foreach (IAPI? api in ApiCollection.Values)
            {
                if (api != null)
                {
                    Reload(intents, api);
                }
            }
        }

        public static void ReloadFailedApis()
        {
            ReloadApis(FailedIntents);
        }

        private static void Reload(ApiIntents allowed, IAPI desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired.ApiType))
            {
                return;
            }

            DeinitBody(desired, delay);
            InitBody(desired, delay);
        }

        /// <summary>
        /// Get all tracks from specified URL
        /// </summary>
        /// <param name="query">URL</param>
        /// <returns>List of tracks</returns>
        /// <exception cref="ArgumentNullException">Throws if query is invalid</exception>
        /// <exception cref="InvalidOperationException">Throws if no results found</exception>
        public static IEnumerable<ITrackInfo> GetAll(string query)
        {
            IEnumerable<ITrackInfo> tracks;

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query), "Invalid query");
            }

            try
            {
                tracks = QueryIdentifier.Execute(query);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Query execution failed", ex);
            }

            if (!tracks.Any())
            {
                throw new InvalidOperationException("No results");
            }

            return tracks;
        }
    }
}
