global using YoutubeApiWrapper = MyGreatestBot.ApiClasses.Music.Youtube.YoutubeApiWrapper;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    public sealed class YoutubeApiWrapper : ITextMusicAPI, IUrlMusicAPI, ISearchMusicAPI
    {
        private YoutubeClient? api;
        private readonly YoutubeApiException GenericException = new();

        private VideoClient Videos => api?.Videos ?? throw GenericException;
        public StreamClient Streams => api?.Videos.Streams ?? throw GenericException;
        private PlaylistClient Playlists => api?.Playlists ?? throw GenericException;
        private SearchClient Search => api?.Search ?? throw GenericException;

        private static class YoutubeQueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex VIDEO_RE = new("/watch\\?v=([^&]+)");
            private static readonly Regex PLAYLIST_RE = new("[&?]list=([^&]+)");
            private static readonly Regex REDUCED_RE = new("youtu\\.be/([^\\?]+)");
            private static readonly Regex TIMING_RE = new("[&?]t=([\\d]+)");
#pragma warning restore SYSLIB1045

            internal static string? TryGetPlaylistId(string query)
            {
                return PLAYLIST_RE.GetMatchValue(query);
            }

            internal static string? TryGetVideoId(string query)
            {
                return VIDEO_RE.GetMatchValue(query);
            }

            internal static string? TryGetReducedId(string query)
            {
                return REDUCED_RE.GetMatchValue(query);
            }

            internal static string? GetTiming(string query)
            {
                return TIMING_RE.GetMatchValue(query);
            }
        }

        private YoutubeApiWrapper()
        {

        }

        private static readonly YoutubeApiWrapper _instance = new();

        public static YoutubeApiWrapper Instance { get; } = _instance;
        public static IUrlMusicAPI UrlMusicInstance { get; } = _instance;
        public static ISearchMusicAPI SearchMusicInstance { get; } = _instance;
        public static ITextMusicAPI TextMusicInstance { get; } = _instance;

        ApiIntents IAPI.ApiType => ApiIntents.Youtube;

        DomainCollection IAccessible.Domains { get; } = "https://www.youtube.com/";

        void IAPI.PerformAuth()
        {
            GoogleCredentialsJSON user = ConfigManager.GetGoogleCredentialsJSON();
            FileStream fileStream = ConfigManager.GetGoogleClientSecretsFileStream();
            _ = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.FromStream(fileStream).Secrets,
                scopes: [YouTubeService.Scope.YoutubeReadonly],
                user: user.Username,
                taskCancellationToken: CancellationToken.None).GetAwaiter().GetResult();

            YouTubeService GoogleService = new(new BaseClientService.Initializer()
            {
                ApiKey = user.Key,
                ApplicationName = typeof(YoutubeApiWrapper).Name
            });

            api = new(GoogleService.HttpClient);
        }

        void IAPI.Logout()
        {
            api = null;
        }

        IEnumerable<ITrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string url)
        {
            if (api == null)
            {
                throw GenericException;
            }

            List<ITrackInfo> tracks = [];

            if (string.IsNullOrWhiteSpace(url))
            {
                return tracks;
            }

            while (true)
            {
                string? playlist_id = YoutubeQueryDecomposer.TryGetPlaylistId(url);

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
                string? video_id = YoutubeQueryDecomposer.TryGetVideoId(url);

                if (string.IsNullOrWhiteSpace(video_id))
                {
                    video_id = YoutubeQueryDecomposer.TryGetReducedId(url);
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

                ITrackInfo? track = UrlMusicInstance.GetTrackFromId(video_id, time);

                if (track != null)
                {
                    tracks.Add(track);
                }

                return tracks;
            }

            return null;
        }

        ITrackInfo? IMusicAPI.GetTrackFromId(string id, int time)
        {
            Video origin = Videos.GetAsync(id)
                    .AsTask()
                    .GetAwaiter()
                    .GetResult();

            if (origin == null)
            {
                return null;
            }

#pragma warning disable CA1859
            ITrackInfo track = new YoutubeTrackInfo(origin);
#pragma warning restore CA1859

            if (time > 0)
            {
                track.PerformSeek(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        ITrackInfo? ISearchMusicAPI.SearchTrack(ITrackInfo other)
        {
            if (other.TrackType == UrlMusicInstance.ApiType)
            {
                return other;
            }

            IEnumerable<VideoSearchResult>? search = Search
                ?.GetVideosAsync($"{other.Title} - {string.Join(", ", other.ArtistArr.Select(a => a.Title))}")
                ?.ToBlockingEnumerable();

            if (search == null)
            {
                return null;
            }

            VideoSearchResult? result = search.Where(track => track != null)
                .FirstOrDefault(track =>
                    Math.Abs((track.Duration.GetValueOrDefault() - other.Duration).Ticks) <= ISearchMusicAPI.MaximumTimeDifference.Ticks);

            return result == null ? null : new YoutubeTrackInfo(result);
        }

        IEnumerable<ITrackInfo> ITextMusicAPI.GetTracksFromPlainText(string text)
        {
            List<ITrackInfo> tracks = [];
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
