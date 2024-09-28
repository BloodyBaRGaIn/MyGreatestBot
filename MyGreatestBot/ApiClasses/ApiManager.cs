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
        private static readonly Dictionary<ApiIntents, IAPI> ApiCollection = [];

        private static ApiIntents InitIntents { get; set; } = ApiIntents.None;
        private static ApiIntents FailedIntents { get; set; } = ApiIntents.None;
        private static ApiIntents EssentialFailedIntents { get; set; } = ApiIntents.None;
        private static ApiIntents RegisteredIntents => ApiCollection.Keys.Aggregate(static (a, b) => a | b);

        public static bool IsAnyEssentialApiFailed => EssentialFailedIntents != ApiIntents.None;
        public static bool IsAnyApiFailed => FailedIntents != ApiIntents.None;

        private enum ApiStatus
        {
            Success,
            Failed,
            Deinit,
            InitSkip,
            DeinitSkip
        }

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

            static void InternalInitAction(IAPI api)
            {
                Init(api);
            }

            DiscordWrapper.CurrentDomainLogHandler.Send("Optional APIs init", LogLevel.Debug);
            ParallelApiAction(InternalInitAction, static api => !api.IsEssential);
            DiscordWrapper.CurrentDomainLogHandler.Send("Essential APIs init", LogLevel.Debug);
            ParallelApiAction(InternalInitAction, static api => api.IsEssential);
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

            void LocalDeinitAction(IAPI api)
            {
                Deinit(intents, api, 1);
            }

            DiscordWrapper.CurrentDomainLogHandler.Send("Optional APIs deinit", LogLevel.Debug);
            ParallelApiAction(LocalDeinitAction, static api => !api.IsEssential);
            DiscordWrapper.CurrentDomainLogHandler.Send("Essential APIs deinit", LogLevel.Debug);
            ParallelApiAction(LocalDeinitAction, static api => api.IsEssential);
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
                DiscordWrapper.CurrentDomainLogHandler.Send(
                    GetApiStatusString(desired.ApiType, ApiStatus.Deinit));
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

        private static string GetApiStatusString(ApiIntents intents, ApiStatus status)
        {
            return $"{intents} {status}.";
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
                    ApiCollection.Where(static record => record.Value != null && !record.Value.IsEssential)
                                 .Select(static record => record.Key)
                                 .Where(IsApiRegisterdAndAllowed)
                                 .Select(static intents =>
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
