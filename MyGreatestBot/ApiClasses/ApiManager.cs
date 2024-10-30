using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API configuration.
    /// </summary>
    public static class ApiManager
    {
        private static readonly Dictionary<ApiIntents, IAPI> ApiCollection = [];

        private static ApiIntents InitIntents { get; set; } = ApiIntents.None;

        private static ApiIntents GetFailedFailedIntents(bool onlyEssential)
        {
            IEnumerable<IAPI> failedapis = ApiCollection.Values
                .Where(a => (!onlyEssential || a.IsEssential) && a.Status == ApiStatus.Failed);

            if (!failedapis.Any())
            {
                return ApiIntents.None;
            }

            return failedapis
                .Select(static a => a.ApiType)
                .Aggregate(static (a, b) => a | b);
        }

        private static ApiIntents FailedIntents => GetFailedFailedIntents(onlyEssential: false);
        private static ApiIntents EssentialFailedIntents => GetFailedFailedIntents(onlyEssential: true);

        private static ApiIntents RegisteredIntents => ApiCollection.Keys.Aggregate(static (a, b) => a | b);

        public static bool IsAnyEssentialApiFailed => EssentialFailedIntents != ApiIntents.None;
        public static bool IsAnyApiFailed => FailedIntents != ApiIntents.None;

        public static void Add([DisallowNull] IAPI api)
        {
            ArgumentNullException.ThrowIfNull(api, nameof(api));
            ApiCollection.Add(api.ApiType, api);
        }

        public static void InitApis()
        {
            InitApis(RegisteredIntents);
        }

        public static void InitApis(ApiIntents intents)
        {
            intents &= RegisteredIntents;

            void InternalInitAction(IAPI api)
            {
                Init(intents, api);
            }

            ParallelApiAction(InternalInitAction, api => !api.IsEssential);
            ParallelApiAction(InternalInitAction, api => api.IsEssential);
        }

        private static void ParallelApiAction(Action<IAPI> action, Predicate<IAPI>? predicate = null)
        {
            ParallelLoopResult result = Parallel.ForEach(
                ApiCollection.Values, api =>
            {
                if (api != null && (predicate?.Invoke(api) ?? true))
                {
                    action(api);
                }
            });

            while (!result.IsCompleted)
            {
                Task.Delay(10).Wait();
            }
        }

        private static void Init(ApiIntents allowed, IAPI desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired.ApiType))
            {
                switch (desired.Status)
                {
                    case ApiStatus.NotInitialized:
                        desired.SetStatus(ApiStatus.InitSkip);
                        break;
                }
            }
            else
            {
                switch (desired.Status)
                {
                    case ApiStatus.InitSkip:
                        desired.SetStatus(ApiStatus.NotInitialized);
                        break;
                }
            }

            if (desired.Status == ApiStatus.Failed)
            {
                DeinitBody(desired, delay);
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
                switch (desired.Status)
                {
                    case ApiStatus.NotInitialized:
                    case ApiStatus.Failed:
                        if (desired is IAccessible accessible)
                        {
                            accessible.TryAccess();
                        }
                        break;
                }

                InitIntents |= desired.ApiType;
                desired.PerformAuth();
            }
            catch (Exception ex)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
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

            void LocalDeinitAction(IAPI api)
            {
                Deinit(intents, api, 1);
            }

            ParallelApiAction(LocalDeinitAction, static api => !api.IsEssential);
            ParallelApiAction(LocalDeinitAction, static api => api.IsEssential);
        }

        private static void Deinit(ApiIntents allowed, IAPI desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired.ApiType))
            {
                return;
            }

            switch (desired.Status)
            {
                case ApiStatus.NotInitialized:
                case ApiStatus.InitSkip:
                    desired.SetStatus(ApiStatus.DeinitSkip);
                    break;
            }

            DeinitBody(desired, delay);
        }

        private static void DeinitBody(IAPI desired, int delay)
        {
            InitIntents &= ~desired.ApiType;
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
            intents &= RegisteredIntents;

            void InternalReloadAction(IAPI api)
            {
                Reload(intents, api);
            }

            ParallelApiAction(InternalReloadAction);
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
        public static IEnumerable<BaseTrackInfo> GetAll(string query)
        {
            IEnumerable<BaseTrackInfo>? tracks;

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

        public static BaseTrackInfo? GetRadio(ApiIntents intents, string id)
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

        private static T? GetApi<T>(ApiIntents intents) where T : class, IAPI
        {
            return ApiCollection[intents] as T;
        }

        public static string GetRegisteredApiStatus()
        {
            try
            {
                return string.Join(
                Environment.NewLine,
                    ApiCollection.Where(static record =>
                    {
                        return record.Value != null &&
                            !record.Value.IsEssential
                            && IsApiRegisterdAndAllowed(record.Key);
                    }).Select(static record => record.Value.GetApiStatusString()));
            }
            catch
            {
                return string.Empty;
            }
        }

        public static ITrackDatabaseAPI? GetDbApiInstance()
        {
            return IsApiRegisterdAndAllowed(ApiIntents.NoSql)
                ? GetApi<ITrackDatabaseAPI>(ApiIntents.NoSql)
                : null;
        }

        public static IMusicAPI? GetMusicApiInstance(ApiIntents apiIntents)
        {
            return IsApiRegisterdAndAllowed(apiIntents)
                ? GetApi<IMusicAPI>(apiIntents)
                : null;
        }
    }
}
