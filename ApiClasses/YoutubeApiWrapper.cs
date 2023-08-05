using DicordNET.Config;
using DicordNET.TrackClasses;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Services;
using Google.Apis.YouTube.v3;
using System.Text.RegularExpressions;
using YoutubeExplode;
using YoutubeExplode.Common;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DicordNET.ApiClasses
{
    internal static class YoutubeApiWrapper
    {
        private static UserCredential? GoogleUserCredential;
        private static YouTubeService? GoogleService;
        internal static YoutubeClient? YoutubeClientInstance { get; private set; }

        internal static VideoClient Videos => YoutubeClientInstance?.Videos ?? throw new ArgumentNullException(nameof(VideoClient));
        internal static StreamClient Streams => YoutubeClientInstance?.Videos.Streams ?? throw new ArgumentNullException(nameof(StreamClient));
        internal static PlaylistClient Playlists => YoutubeClientInstance?.Playlists ?? throw new ArgumentNullException(nameof(PlaylistClient));

        private static class YoutubeQueryDecomposer
        {
            private static readonly Regex VIDEO_RE = new("/watch\\?v=([^&]+)");
            private static readonly Regex PLAYLIST_RE = new("[&?]list=([^&]+)");

            internal static string? TryGetPlaylistId(string query)
            {
                return PLAYLIST_RE.GetMatchValue(query);
            }

            internal static string? TryGetVideoId(string query)
            {
                return VIDEO_RE.GetMatchValue(query);
            }
        }

        internal static void Init()
        {
            GoogleCredentialsJSON user = ConfigManager.GetGoogleCredentialsJSON();
            FileStream fileStream = ConfigManager.GetGoogleClientSecretsFileStream();

            GoogleUserCredential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                clientSecrets: GoogleClientSecrets.FromStream(fileStream).Secrets,
                scopes: new string[1] { YouTubeService.Scope.YoutubeReadonly },
                user: user.Username,
                taskCancellationToken: CancellationToken.None).GetAwaiter().GetResult();

            GoogleService = new(new BaseClientService.Initializer()
            {
                ApiKey = user.Key,
                ApplicationName = typeof(YoutubeApiWrapper).ToString()
            });

            YoutubeClientInstance = new(GoogleService.HttpClient);
        }

        internal static List<YoutubeTrackInfo> GetTracks(string? query)
        {
            if (YoutubeClientInstance == null)
            {
                throw new ArgumentNullException(nameof(YoutubeClientInstance));
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
                                                .GetAwaiter()
                                                .GetResult();

                if (pl_instance != null)
                {
                    IReadOnlyList<PlaylistVideo> playlist_videos = Playlists.GetVideosAsync(pl_instance.Id)
                                                                            .GetAwaiter()
                                                                            .GetResult();

                    foreach (var pl_video in playlist_videos)
                    {
                        tracks.Add(new(pl_video, pl_instance));
                    }
                }

                return tracks;
            }

            string? video_id = YoutubeQueryDecomposer.TryGetVideoId(query);

            if (!string.IsNullOrWhiteSpace(video_id))
            {
                var video = Videos.GetAsync(video_id)
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
