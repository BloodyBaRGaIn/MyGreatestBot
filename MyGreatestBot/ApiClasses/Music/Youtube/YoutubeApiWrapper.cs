using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
using Video = YoutubeExplode.Videos.Video;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    public sealed class YoutubeApiWrapper : IMusicAPI, ISearchable
    {
        [AllowNull]
        private YoutubeClient api;
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

        public static IMusicAPI Instance { get; private set; } = new YoutubeApiWrapper();

        ApiIntents IAPI.ApiType => ApiIntents.Youtube;

        DomainCollection IAccessible.Domains { get; } = new("https://www.youtube.com/");

        public void PerformAuth()
        {
            GoogleCredentialsJSON user = ConfigManager.GetGoogleCredentialsJSON();
            FileStream fileStream = ConfigManager.GetGoogleClientSecretsFileStream();
            _ = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.FromStream(fileStream).Secrets,
                scopes: new string[1] { YouTubeService.Scope.YoutubeReadonly },
                user: user.Username,
                taskCancellationToken: CancellationToken.None).GetAwaiter().GetResult();

            YouTubeService GoogleService = new(new BaseClientService.Initializer()
            {
                ApiKey = user.Key,
                ApplicationName = typeof(YoutubeApiWrapper).Name
            });

            api = new(GoogleService.HttpClient);
        }

        public void Logout()
        {
            api = null;
        }

        public IEnumerable<ITrackInfo>? GetTracks(string query)
        {
            if (api == null)
            {
                throw GenericException;
            }

            List<ITrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            while (true)
            {
                string? playlist_id = YoutubeQueryDecomposer.TryGetPlaylistId(query);

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
                string? video_id = YoutubeQueryDecomposer.TryGetVideoId(query);

                if (string.IsNullOrWhiteSpace(video_id))
                {
                    video_id = YoutubeQueryDecomposer.TryGetReducedId(query);
                }
                if (string.IsNullOrWhiteSpace(video_id))
                {
                    break;
                }

                string? timing = YoutubeQueryDecomposer.GetTiming(query);
                if (string.IsNullOrWhiteSpace(timing) || !int.TryParse(timing, out int time))
                {
                    time = 0;
                }

                ITrackInfo? track = GetTrack(video_id, time);

                if (track != null)
                {
                    tracks.Add(track);
                }

                return tracks;
            }

            return null;
        }

        public ITrackInfo? GetTrack(string id, int time = 0)
        {
            Video video = Videos.GetAsync(id)
                    .AsTask()
                    .GetAwaiter()
                    .GetResult();

            if (video == null)
            {
                return null;
            }

            ITrackInfo track = new YoutubeTrackInfo(video);

            if (time > 0)
            {
                track.PerformSeek(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        public ITrackInfo? SearchTrack(ITrackInfo other)
        {
            if (other.TrackType == ApiIntents.Youtube)
            {
                return other;
            }

            IAsyncEnumerable<VideoSearchResult> search =
                Search.GetVideosAsync($"{other.Title} - {string.Join(", ", other.ArtistArr.Select(a => a.Title))}");

            if (search == null)
            {
                return null;
            }

            VideoSearchResult? result = null;

            Task.Run(async () =>
            {
                await foreach (VideoSearchResult track in search)
                {
                    TimeSpan found_duration = track.Duration.GetValueOrDefault();
                    TimeSpan diff = found_duration - other.Duration;
                    if (Math.Abs(diff.Ticks) > TimeSpan.FromSeconds(2).Ticks)
                    {
                        continue;
                    }
                    result = track;
                    return;
                }
            }).Wait();

            if (result == null)
            {
                return null;
            }

            return new YoutubeTrackInfo(result);
        }

        public IEnumerable<ITrackInfo> GetTracksSearch(string query)
        {
            List<ITrackInfo> tracks = new();
            IAsyncEnumerable<VideoSearchResult> search =
            Search.GetVideosAsync(query);
            if (search == null)
            {
                return tracks;
            }

            VideoSearchResult? result = null;

            Task.Run(async () =>
            {
                await foreach (VideoSearchResult track in search)
                {
                    result = track;
                    return;
                }
            }).Wait();

            if (result != null)
            {
                tracks.Add(new YoutubeTrackInfo(result));
            }

            return tracks;
        }
    }
}
