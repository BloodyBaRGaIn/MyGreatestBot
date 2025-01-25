global using YoutubeApiWrapper = MyGreatestBot.ApiClasses.Music.Youtube.YoutubeApiWrapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Search;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    /// <summary>
    /// Youtube API wrapper class
    /// </summary>
    public sealed partial class YoutubeApiWrapper : ITextMusicAPI, IUrlMusicAPI, ISearchMusicAPI
    {
        private YoutubeClient? api;
        private readonly YoutubeApiException GenericException = new();

        private VideoClient Videos => api?.Videos ?? throw GenericException;
        public StreamClient Streams => api?.Videos.Streams ?? throw GenericException;
        private PlaylistClient Playlists => api?.Playlists ?? throw GenericException;
        private SearchClient Search => api?.Search ?? throw GenericException;

        private static partial class YoutubeQueryDecomposer
        {
            private static readonly Regex VideoRegex = GenerateVideoRegex();
            private static readonly Regex ReducedVideoRegex = GenerateReducedVideoRegex();
            private static readonly Regex PlaylistRegex = GeneratePlaylistRegex();
            private static readonly Regex TimingRegex = GenerateTimingRegex();

            internal static string? TryGetPlaylistId(string query)
            {
                return PlaylistRegex.GetMatchValue(query);
            }

            internal static string? TryGetVideoId(string query)
            {
                return VideoRegex.GetMatchValue(query);
            }

            internal static string? TryGetReducedId(string query)
            {
                return ReducedVideoRegex.GetMatchValue(query);
            }

            internal static string? GetTiming(string query)
            {
                return TimingRegex.GetMatchValue(query);
            }

            [GeneratedRegex("/watch\\?v=([^&]+)")]
            private static partial Regex GenerateVideoRegex();

            [GeneratedRegex("youtu\\.be/([^?]+)")]
            private static partial Regex GenerateReducedVideoRegex();

            [GeneratedRegex("[&?]list=([^&]+)")]
            private static partial Regex GeneratePlaylistRegex();

            [GeneratedRegex("[&?]t=([\\d]+)")]
            private static partial Regex GenerateTimingRegex();
        }

        private YoutubeApiWrapper()
        {

        }

        public static YoutubeApiWrapper Instance { get; } = new();
        public static IUrlMusicAPI UrlMusicInstance => Instance;
        public static ISearchMusicAPI SearchMusicInstance => Instance;
        public static ITextMusicAPI TextMusicInstance => Instance;

        ApiIntents IAPI.ApiType => ApiIntents.Youtube;
        ApiStatus IAPI.OldStatus { get; set; }

        DomainCollection IAccessible.Domains { get; } = "https://www.youtube.com/";

        void IAPI.PerformAuthInternal()
        {
            GoogleCredentialsJSON user = ConfigManager.GetGoogleCredentialsJSON();
            using FileStream fileStream = ConfigManager.GetGoogleClientSecretsFileStream();
            IDataStore dataStore = new FileDataStore(Directory.GetCurrentDirectory());

            UserCredential credentials = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.FromStream(fileStream).Secrets,
                scopes: [YouTubeService.Scope.Youtube],
                user: user.Username,
                taskCancellationToken: CancellationToken.None,
                dataStore: dataStore).GetAwaiter().GetResult();

            _ = credentials.RefreshTokenAsync(CancellationToken.None).GetAwaiter().GetResult();

            YouTubeService GoogleService = new(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credentials,
                ApiKey = user.Key,
                ApplicationName = string.Empty,
                MaxUrlLength = 0,

                //"MyGreatestApp" //"MyGreatestProject" //"MyGreatestClient" //"API Youtube key"
            });

            HttpClient httpClient = GoogleService.HttpClient;
            //httpClient.DefaultRequestHeaders.ProxyAuthorization = new AuthenticationHeaderValue(
            //    credentials.Token.TokenType,
            //    credentials.Token.AccessToken); // AccessToken // RefreshToken // IdToken
            //httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(
            //    credentials.Token.TokenType,
            //    credentials.Token.AccessToken); // AccessToken // RefreshToken // IdToken

            api = new(httpClient);
        }

        void IAPI.LogoutInternal()
        {
            api = null;
        }

        IEnumerable<BaseTrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string url)
        {
            if (api == null)
            {
                throw GenericException;
            }

            List<BaseTrackInfo> tracks = [];

            if (string.IsNullOrWhiteSpace(url))
            {
                return tracks;
            }

            while (true)
            {
                string playlist_id = YoutubeQueryDecomposer.TryGetPlaylistId(url).EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(playlist_id))
                {
                    break;
                }

                Playlist pl_instance = Playlists.GetAsync(playlist_id)
                                                .AsTask()
                                                .GetAwaiter()
                                                .GetResult();

                if (pl_instance == null)
                {
                    // retry query parsing as video
                    // if cannot get playlist content somehow
                    break;
                }

                IReadOnlyList<PlaylistVideo> playlist_videos = Playlists.GetVideosAsync(pl_instance.Id)
                                                                        .GetAwaiter()
                                                                        .GetResult();

                foreach (PlaylistVideo pl_video in playlist_videos)
                {
                    tracks.Add(new YoutubeTrackInfo(pl_video, pl_instance));
                }

                return tracks;
            }

            while (true)
            {
                string video_id = YoutubeQueryDecomposer.TryGetVideoId(url).EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(video_id))
                {
                    video_id = YoutubeQueryDecomposer.TryGetReducedId(url).EnsureIdentifier();
                }
                if (string.IsNullOrWhiteSpace(video_id))
                {
                    break;
                }

                string? timing = YoutubeQueryDecomposer.GetTiming(url);
                if (string.IsNullOrWhiteSpace(timing) || !int.TryParse(timing, out int time))
                {
                    time = 0;
                }

                BaseTrackInfo? track = UrlMusicInstance.GetTrackFromId(video_id, time);

                if (track != null)
                {
                    tracks.Add(track);
                }

                return tracks;
            }

            return null;
        }

        BaseTrackInfo? IMusicAPI.GetTrackFromId(string id, int time)
        {
            id = id.EnsureIdentifier();

            Video origin = Videos.GetAsync(id)
                    .AsTask()
                    .GetAwaiter()
                    .GetResult();

            if (origin == null)
            {
                return null;
            }

            BaseTrackInfo track = new YoutubeTrackInfo(origin);
            if (time > 0)
            {
                track.PerformRewind(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        BaseTrackInfo? ISearchMusicAPI.SearchTrack(BaseTrackInfo other)
        {
            if (other.TrackType == UrlMusicInstance.ApiType)
            {
                return other;
            }

            const string searchSeparator = "-";
            const char searchComma = ',';

            string searchQuery = $"{other.Title} {searchSeparator} {string.Join($"{searchComma} ", other.ArtistArr.Select(a => a.Title))}";

            static IEnumerable<string> GetWords(string input)
            {
                return input
                    .ToLowerInvariant()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Where(s => s != searchSeparator)
                    .Select(s => s.Trim(searchComma))
                    .Where(string.IsNullOrEmpty);
            }

            IEnumerable<string> searchWords = GetWords(searchQuery);
            int searchWordsCount = searchWords.Count();

            IEnumerable<VideoSearchResult>? search = Search
                ?.GetVideosAsync(searchQuery)
                ?.ToBlockingEnumerable();

            if (search == null)
            {
                return null;
            }

            VideoSearchResult? result = search.Where(track => track != null)
                .FirstOrDefault(track =>
                {
                    IEnumerable<string> resultWords = GetWords(track.Title);

                    return Math.Abs((track.Duration.GetValueOrDefault() - other.Duration).Ticks)
                            <= ISearchMusicAPI.MaximumTimeDifference.Ticks
                        && Math.Abs(resultWords.Count() - searchWordsCount) < 2
                        && searchWords.Intersect(resultWords).Count() == searchWordsCount;
                });

            return result == null ? null : new YoutubeTrackInfo(result);
        }

        IEnumerable<BaseTrackInfo> ITextMusicAPI.GetTracksFromPlainText(string text)
        {
            List<BaseTrackInfo> tracks = [];
            IEnumerable<VideoSearchResult>? search = Search?.GetVideosAsync(text)?.ToBlockingEnumerable();
            if (search == null)
            {
                return tracks;
            }

            VideoSearchResult? result = search.FirstOrDefault();

            if (result != null)
            {
                tracks.Add(new YoutubeTrackInfo(result));
            }

            return tracks;
        }
    }
}
