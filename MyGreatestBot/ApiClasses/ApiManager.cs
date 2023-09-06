using MyGreatestBot.Extensions;
using MyGreatestBot.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// API configuration
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class ApiManager
    {
        /// <summary>
        /// Intents used on initialization
        /// </summary>
        public static ApiIntents InitIntents { get; private set; } = ApiIntents.None;

        public static ApiIntents FailedIntents { get; private set; } = ApiIntents.None;

        public static readonly Dictionary<ApiIntents, DomainCollection> DoaminsDictionary = new()
        {
            [ApiIntents.Youtube] = new DomainCollection("https://www.youtube.com/"),
            [ApiIntents.Yandex] = new DomainCollection("https://music.yandex.ru/"),
            [ApiIntents.Vk] = new DomainCollection("https://www.vk.com/"),
            [ApiIntents.Spotify] = new DomainCollection("https://open.spotify.com/", string.Empty),
            [ApiIntents.Discord] = new DomainCollection("https://www.discord.com/")
        };

        public static void TryAcessDomain(ApiIntents api)
        {
            if (!DoaminsDictionary.TryGetValue(api, out DomainCollection? domains)
                || !domains.Any())
            {
                return;
            }

            foreach (string url in domains)
            {
                if (url == string.Empty)
                {
                    // bypass accessing check
                    return;
                }
                HttpClient client = new();
                try
                {
                    HttpResponseMessage message = client.Send(new HttpRequestMessage(HttpMethod.Get, domains));

                    using StreamReader stream = new(message.Content.ReadAsStream());
                    string content = stream.ReadToEnd();
                    stream.Close();

                    if (message.IsSuccessStatusCode)
                    {
                        return;
                    }
                }
                catch { }
                finally
                {
                    client.Dispose();
                }
            }

            throw new ApplicationException($"{api} is not available");
        }

        private static AuthActions GetActions(ApiIntents desired)
        {
            if (!AuthActions.TryGetValue(desired, out AuthActions? actions))
            {
                throw new ArgumentException($"{desired} auth actions not provided");
            }
            return actions;
        }

        /// <summary>
        /// <para>Performs auth for APIs</para>
        /// <para>Throws an exception if API auth process failed</para>
        /// </summary>
        /// <param name="intents">APIs intents for initialization</param>
        /// <exception cref="ApplicationException"></exception>
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

            foreach (ApiIntents api in AuthActions.ApiOrder)
            {
                Init(api);
            }
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

            InitBody(desired, GetActions(desired), delay);
        }

        private static void InitBody(ApiIntents desired, AuthActions actions, int delay)
        {
            try
            {
                TryAcessDomain(desired);
                actions.InitAction.Invoke();
                Console.WriteLine($"{desired} SUCCESS");
                FailedIntents &= ~desired;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"{desired} FAILED");
                Console.Error.WriteLine(ex.GetExtendedMessage());
                try
                {
                    actions.DeinitAction.Invoke();
                }
                catch { }
                FailedIntents |= desired;
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
        public static void DeinitApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            foreach (ApiIntents api in AuthActions.ApiOrder)
            {
                Deinit(intents, api);
            }
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

            DeinitBody(desired, GetActions(desired), delay);
        }

        private static void DeinitBody(ApiIntents desired, AuthActions actions, int delay)
        {
            try
            {
                actions.DeinitAction.Invoke();
            }
            catch
            {
                _ = desired;
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
        public static void ReloadApis(ApiIntents intents = ApiIntents.All)
        {
            intents &= InitIntents;

            foreach (ApiIntents api in AuthActions.ApiOrder)
            {
                Reload(intents, api);
            }
        }

        public static void ReloadFailedApis()
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

            AuthActions actions = GetActions(desired);

            DeinitBody(desired, actions, delay);
            InitBody(desired, actions, delay);
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
                tracks = Music.QueryIdentifier.Execute(query);
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
