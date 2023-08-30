using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.ApiClasses.Services.Sql;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API configuration
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class ApiManager
    {
        /// <summary>
        /// Intents used on initialization
        /// </summary>
        internal static ApiIntents InitIntents { get; private set; } = ApiIntents.None;

        internal static ApiIntents FailedIntents { get; private set; } = ApiIntents.None;

        private sealed class AuthActions
        {
            internal readonly Action InitAction;
            internal readonly Action DeinitAction;

            internal AuthActions(Action initAction, Action deinitAction)
            {
                InitAction = initAction;
                DeinitAction = deinitAction;
            }
        }

        private static readonly Dictionary<ApiIntents, AuthActions> AuthActionsDictionary = new()
        {
            [ApiIntents.Youtube] = new AuthActions(YoutubeApiWrapper.PerformAuth, YoutubeApiWrapper.Logout),
            [ApiIntents.Yandex] = new AuthActions(YandexApiWrapper.PerformAuth, YandexApiWrapper.Logout),
            [ApiIntents.Vk] = new AuthActions(VkApiWrapper.PerformAuth, VkApiWrapper.Logout),
            [ApiIntents.Spotify] = new AuthActions(SpotifyApiWrapper.PerformAuth, SpotifyApiWrapper.Logout),
            [ApiIntents.Sql] = new AuthActions(SqlServerWrapper.Open, SqlServerWrapper.Close)
        };

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

            if ((InitIntents & ApiIntents.Youtube) != 0)
            {
                // YoutubeExplode won't work without specific environment variable
                YoutubeExplodeBypass.Bypass();
            }

            Init(ApiIntents.Sql);
            Init(ApiIntents.Youtube);
            Init(ApiIntents.Yandex);
            Init(ApiIntents.Vk);
            Init(ApiIntents.Spotify);
        }

        /// <summary>
        /// Ititializes desired API
        /// </summary>
        /// <param name="desired">API flag</param>
        /// <param name="init_action">Init action</param>
        /// <param name="delay">Sleep after invocation</param>
        /// <exception cref="ApplicationException">Throws if failed</exception>
        private static void Init(ApiIntents desired, int delay = 500)
        {
            if (!InitIntents.HasFlag(desired))
            {
                return;
            }

            if (!AuthActionsDictionary.TryGetValue(desired, out AuthActions? actions))
            {
                throw new ArgumentException(nameof(ApiIntents));
            }

            try
            {
                actions.InitAction.Invoke();
                Console.WriteLine($"{desired} SUCCESS");
                FailedIntents &= ~desired;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{desired} FAILED");
                Console.Error.WriteLine(ex.GetExtendedMessage());
                actions.DeinitAction.Invoke();
                FailedIntents |= desired;
                //throw new ApplicationException($"{desired} auth failed", ex);
            }
            finally
            {
                Task.Delay(delay).Wait();
            }
        }

        /// <summary>
        /// <para>Performs logout</para>
        /// <para>Throws an exception if API logout process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for logout</param>
        internal static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            Deinit(intents, ApiIntents.Sql);
            Deinit(intents, ApiIntents.Youtube);
            Deinit(intents, ApiIntents.Yandex);
            Deinit(intents, ApiIntents.Vk);
            Deinit(intents, ApiIntents.Spotify);
        }

        /// <summary>
        /// Deinit API
        /// </summary>
        /// <param name="allowed">Alowed APIs</param>
        /// <param name="desired">API flag</param>
        /// <param name="deinit_action">Deinit action</param>
        /// <param name="delay">Delay after invocation</param>
        private static void Deinit(ApiIntents allowed, ApiIntents desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired))
            {
                return;
            }

            if (!AuthActionsDictionary.TryGetValue(desired, out AuthActions? actions))
            {
                throw new ArgumentException(nameof(ApiIntents));
            }

            try
            {
                actions.DeinitAction.Invoke();
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
        /// <para>Performs re-auth for APIs</para>
        /// <para>Throws an exception if API re-auth process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for reinitialization</param>
        /// <exception cref="ApplicationException"></exception>
        internal static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            Reload(intents, ApiIntents.Sql);
            Reload(intents, ApiIntents.Youtube);
            Reload(intents, ApiIntents.Yandex);
            Reload(intents, ApiIntents.Vk);
            Reload(intents, ApiIntents.Spotify);
        }

        internal static void ReloadFailedApis()
        {
            ReloadApis(FailedIntents);
        }

        /// <summary>
        /// Reloads API
        /// </summary>
        /// <param name="allowed">Allowed APIs</param>
        /// <param name="desired">API flag</param>
        /// <param name="init_action">Init action</param>
        /// <param name="deinit_action">Deinit action</param>
        /// <param name="delay">Delay after invocation</param>
        private static void Reload(ApiIntents allowed, ApiIntents desired, int delay = 500)
        {
            if (!allowed.HasFlag(desired))
            {
                return;
            }

            if (!AuthActionsDictionary.TryGetValue(desired, out AuthActions? actions))
            {
                throw new ArgumentException(nameof(ApiIntents));
            }

            try
            {
                actions.DeinitAction.Invoke();
            }
            catch
            {
                ;
            }
            finally
            {
                Task.Delay(delay).Wait();
            }

            try
            {
                actions.InitAction.Invoke();
                Console.WriteLine($"{desired} SUCCESS");
                FailedIntents &= ~desired;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{desired} FAILED");
                Console.Error.WriteLine(ex.GetExtendedMessage());
                actions.DeinitAction.Invoke();
                FailedIntents |= desired;
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
        /// <exception cref="ArgumentNullException">Throws if query is invalid</exception>
        /// <exception cref="InvalidOperationException">Throws if no results found</exception>
        internal static IEnumerable<ITrackInfo> GetAll(string? query)
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

        /// <summary>
        /// Links identifier class
        /// </summary>
        [SupportedOSPlatform("windows")]
        private static class QueryIdentifier
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex YOUTUBE_RE = new("^((http([s])?://)?((www|m)\\.)?((youtube\\.([\\w])+)|(youtu\\.be))/)");
            private static readonly Regex YANDEX_RE = new("^((http([s])?://)?music\\.yandex\\.([\\w])+/)");
            private static readonly Regex VK_RE = new("^((http([s])?://)?((www|m)\\.)?vk\\.com/)");
            private static readonly Regex SPOTIFY_RE = new("^((http([s])?://)?open\\.spotify\\.com/)");
#pragma warning restore SYSLIB1045

            internal static IEnumerable<ITrackInfo> Execute(string query)
            {
                return TracksReceiver.Execute(query);
            }

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
                        if (InitIntents.HasFlag(receiver.desired))
                        {
                            return receiver.get_tracks.Invoke(query);
                        }
                        else
                        {
                            throw new GenericApiException(receiver.desired);
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
}
