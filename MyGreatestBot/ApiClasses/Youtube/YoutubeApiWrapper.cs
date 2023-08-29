using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using MyGreatestBot.ConfigStructs;
using MyGreatestBot.Extensions;
using MyGreatestBot.Utils;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MyGreatestBot.ApiClasses.Youtube
{
    /// <summary>
    /// Youtube API wrapper class
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class YoutubeApiWrapper
    {
        private static YoutubeClient? api;

        private static readonly YoutubeApiException GenericException = new();

        internal static VideoClient Videos => api?.Videos ?? throw GenericException;
        internal static StreamClient Streams => api?.Videos.Streams ?? throw GenericException;
        internal static PlaylistClient Playlists => api?.Playlists ?? throw GenericException;

        private static class YoutubeQueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex VIDEO_RE = new("/watch\\?v=([^&]+)");
            private static readonly Regex PLAYLIST_RE = new("[&?]list=([^&]+)");
#pragma warning restore SYSLIB1045

            internal static string? TryGetPlaylistId(string query)
            {
                return PLAYLIST_RE.GetMatchValue(query);
            }

            internal static string? TryGetVideoId(string query)
            {
                return VIDEO_RE.GetMatchValue(query);
            }
        }

        internal static void PerformAuth()
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
                ApplicationName = typeof(YoutubeApiWrapper).ToString()
            });

            api = new(GoogleService.HttpClient);
        }

        internal static void Logout()
        {
            api = null;
        }

        internal static IEnumerable<YoutubeTrackInfo> GetTracks(string? query)
        {
            if (api == null)
            {
                throw GenericException;
            }

            List<YoutubeTrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            string? playlist_id = YoutubeQueryDecomposer.TryGetPlaylistId(query);

            if (!string.IsNullOrWhiteSpace(playlist_id))
            {
                Playlist pl_instance = Playlists.GetAsync(playlist_id)
                                                .AsTask()
                                                .GetAwaiter()
                                                .GetResult();

                if (pl_instance != null)
                {
                    IReadOnlyList<PlaylistVideo> playlist_videos = Playlists.GetVideosAsync(pl_instance.Id)
                                                                            .GetAwaiter()
                                                                            .GetResult();

                    foreach (PlaylistVideo pl_video in playlist_videos)
                    {
                        tracks.Add(new(pl_video, pl_instance));
                    }
                }

                return tracks;
            }

            string? video_id = YoutubeQueryDecomposer.TryGetVideoId(query);

            if (!string.IsNullOrWhiteSpace(video_id))
            {
                Video video = Videos.GetAsync(video_id)
                                  .AsTask()
                                  .GetAwaiter()
                                  .GetResult();

                if (video != null)
                {
                    tracks.Add(new(video));
                }
            }

            return tracks;
        }
    }
}
