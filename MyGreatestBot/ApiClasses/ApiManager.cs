using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API configuration
    /// </summary>
    public static class ApiManager
    {
        private static readonly Dictionary<ApiIntents, IAPI?> ApiCollection = [];

        private static ApiIntents InitIntents { get; set; } = ApiIntents.None;
        private static ApiIntents FailedIntents { get; set; } = ApiIntents.None;
        private static ApiIntents EssentialFailedIntents { get; set; } = ApiIntents.None;
        private static ApiIntents RegisteredIntents => ApiCollection.Keys.Aggregate((a, b) => a | b);

        public static bool IsAnyEssentialApiFailed => EssentialFailedIntents != ApiIntents.None;
        public static bool IsAnyApiFailed => FailedIntents != ApiIntents.None;

        private enum ApiStatus
        {
            Success,
            Failed,
            InitSkip,
            DeinitSkip
        }

        public static void Add([DisallowNull] IAPI api)
        {
            ApiCollection.Add(api.ApiType, api);
        }

        public static T? Get<T>(ApiIntents intents) where T : IAPI
        {
            return (T?)ApiCollection[intents];
        }

        public static void InitApis()
        {
            InitApis(RegisteredIntents);
        }

        public static void InitApis(ApiIntents intents)
        {
            InitIntents = intents;

            if (InitIntents.HasFlag(ApiIntents.Spotify))
            {
                // init Yandex and Youtube API for searching tracks from Spotify
                InitIntents |= ApiIntents.Yandex;
                InitIntents |= ApiIntents.Youtube;
            }

            if (InitIntents.HasFlag(ApiIntents.Youtube))
            {
                // YoutubeExplode won't work without specific environment variable
                YoutubeExplodeBypass.Bypass();
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
                DiscordWrapper.CurrentDomainLogHandler.Send(
                   GetApiStatusString(desired.ApiType, ApiStatus.InitSkip),
                   LogLevel.Debug);

                return;
            }

            InitBody(desired, delay);
        }

        private static void InitBody(IAPI desired, int delay)
        {
            if (desired.IsEssential)
            {
                delay = 1;
            }

            try
            {
                if (desired is IAccessible accessible)
                {
                    accessible.TryAccess();
                }

                desired.PerformAuth();

                DiscordWrapper.CurrentDomainLogHandler.Send(
                    GetApiStatusString(desired.ApiType, ApiStatus.Success));

                FailedIntents &= ~desired.ApiType;
            }
            catch (Exception ex)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(
                    string.Join(Environment.NewLine,
                        GetApiStatusString(desired.ApiType, ApiStatus.Failed),
                        ex.GetExtendedMessage()));

                try
                {
                    desired.Logout();
                }
                catch { }
                FailedIntents |= desired.ApiType;
                if (desired.IsEssential)
                {
                    EssentialFailedIntents |= desired.ApiType;
                }
            }
            finally
            {
                Task.Delay(delay).Wait();
            }
        }

        public static void DeinitApis()
        {
            DeinitApis(RegisteredIntents);
        }

        public static void DeinitApis(ApiIntents intents)
        {
            intents &= InitIntents;

            ParallelLoopResult loopResult = Parallel.ForEach(ApiCollection.Values, api =>
            {
                if (api != null)
                {
                    Deinit(intents, api, 1);
                }
            });

            while (!loopResult.IsCompleted)
            {
                Task.Delay(1).Wait();
            }
        }

        private static void Deinit(ApiIntents allowed, IAPI desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired.ApiType))
            {
                DiscordWrapper.CurrentDomainLogHandler.Send(
                    GetApiStatusString(desired.ApiType, ApiStatus.DeinitSkip),
                    LogLevel.Debug);

                return;
            }

            DeinitBody(desired, delay);
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
            IEnumerable<ITrackInfo>? tracks;

            if (string.IsNullOrWhiteSpace(query))
            {
                throw new ArgumentNullException(nameof(query), "Invalid query");
            }

            try
            {
                tracks = QueryIdentifier.GetTracks(query);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    string.Join(Environment.NewLine,
                        "Query execution failed",
                        ex.Message), ex);
            }

            return tracks is null
                ? throw new InvalidOperationException("Invalid query")
                : !tracks.Any() ? throw new InvalidOperationException("No results") : tracks;
        }

        public static ITrackInfo? GetRadio(ApiIntents intents, string id)
        {
            return QueryIdentifier.GetRadio(intents, id);
        }

        public static bool IsApiRegisterdAndAllowed(ApiIntents intents)
        {
            return InitIntents.HasFlag(intents) && RegisteredIntents.HasFlag(intents);
        }

        public static bool IsApiFailed(ApiIntents intents)
        {
            return IsApiRegisterdAndAllowed(intents) && FailedIntents.HasFlag(intents);
        }

        private static string GetApiStatusString(ApiIntents intents, ApiStatus status)
        {
            return $"{intents} {status}";
        }

        public static string GetRegisteredApiStatus()
        {
            try
            {
                return string.Join(
                Environment.NewLine,
                    ApiCollection
                    .Where(record => record.Value != null && !record.Value.IsEssential)
                    .Select(record => record.Key)
                    .Where(IsApiRegisterdAndAllowed)
                    .Select(intents =>
                        GetApiStatusString(intents, IsApiFailed(intents)
                                                    ? ApiStatus.Failed
                                                    : ApiStatus.Success))
                    );
            }
            catch
            {
                return string.Empty;
            }
        }

        public static ITrackDatabaseAPI? GetDbApiInstance()
        {
            return IsApiRegisterdAndAllowed(ApiIntents.NoSql)
                ? LiteDbWrapper.Instance
                : null;
        }
    }
}
