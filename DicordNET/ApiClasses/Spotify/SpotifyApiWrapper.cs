using DicordNET.Config;
using DicordNET.Extensions;
using SpotifyAPI.Web;
using System.Runtime.Versioning;
//using SpotifyAPI.Web.Auth;
using System.Text.RegularExpressions;

namespace DicordNET.ApiClasses.Spotify
{
    /// <summary>
    /// Spotify API wrapper class
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class SpotifyApiWrapper
    {
        //private static EmbedIOAuthServer? server;
        private static SpotifyClient? SpotifyClientInstance;

        internal static IPlaylistsClient Playlists => SpotifyClientInstance?.Playlists ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static IAlbumsClient Albums => SpotifyClientInstance?.Albums ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static IArtistsClient Artists => SpotifyClientInstance?.Artists ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static ITracksClient Tracks => SpotifyClientInstance?.Tracks ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static IPlayerClient Player => SpotifyClientInstance?.Player ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static ISearchClient Search => SpotifyClientInstance?.Search ?? throw new ArgumentException(nameof(SpotifyClientInstance));

        private static class SpotifyQueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex PLAYLIST_RE = new("/playlist/([^\\?]+)");
            private static readonly Regex ALBUM_RE = new("/album/([^\\?]+)");
            private static readonly Regex ARTIST_RE = new("/artist/([^\\?]+)");
            private static readonly Regex TRACK_RE = new("/track/([^\\?]+)");
#pragma warning restore SYSLIB1045

            internal static string? TryGetPlaylistId(string query)
            {
                return PLAYLIST_RE.GetMatchValue(query);
            }

            internal static string? TryGetAlbumId(string query)
            {
                return ALBUM_RE.GetMatchValue(query);
            }

            internal static string? TryGetArtistId(string query)
            {
                return ARTIST_RE.GetMatchValue(query);
            }

            internal static string? TryGetTrackId(string query)
            {
                return TRACK_RE.GetMatchValue(query);
            }
        }

        internal static void PerformAuth()
        {
            SpotifyClientSecretsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

            //server = new(new Uri("http://localhost:5543/callback"), 5543);
            //server.Start().Wait();

            //server.AuthorizationCodeReceived += Server_AuthorizationCodeReceived;
            //server.ErrorReceived += Server_ErrorReceived;

            //var request = new LoginRequest(server.BaseUri, spotifyClientSecrets.ClientId, LoginRequest.ResponseType.Code)
            //{
            //    Scope = new List<string> { Scopes.UserModifyPlaybackState, Scopes.UserReadPlaybackState, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadEmail }
            //};
            //BrowserUtil.Open(request.ToUri());

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(
                    new ClientCredentialsAuthenticator(
                        spotifyClientSecrets.ClientId,
                        spotifyClientSecrets.ClientSecret
                        )
                    );

            if (SpotifyClientInstance != null)
            {
                lock (SpotifyClientInstance)
                {
                    SpotifyClientInstance = new(config);
                }
            }
            else
            {
                SpotifyClientInstance = new(config);
            }
        }

        //private static Task Server_ErrorReceived(object arg1, string arg2, string? arg3)
        //{
        //    throw new NotImplementedException();
        //}

        //private static async Task Server_AuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        //{
        //    if (server == null)
        //    {
        //        return;
        //    }

        //    await server.Stop();

        //    SpotifyClientSecretsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

        //    var config = SpotifyClientConfig.CreateDefault();
        //    var tokenResponse = await new OAuthClient(config).RequestToken(
        //      new AuthorizationCodeTokenRequest(
        //        spotifyClientSecrets.ClientId, spotifyClientSecrets.ClientSecret, response.Code, new Uri("http://localhost:5543/callback")
        //      )
        //    );

        //    SpotifyClientInstance = new(tokenResponse.AccessToken);
        //}

        internal static IEnumerable<SpotifyTrackInfo> GetTracks(string? query)
        {
            List<SpotifyTrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            {
                string? playlist_id = SpotifyQueryDecomposer.TryGetPlaylistId(query);
                if (!string.IsNullOrWhiteSpace(playlist_id))
                {
                    FullPlaylist? playlist = Playlists.Get(playlist_id).GetAwaiter().GetResult();
                    List<PlaylistTrack<IPlayableItem>>? tracks_list = playlist.Tracks?.Items ?? null;
                    if (tracks_list != null)
                    {
                        IEnumerable<IPlayableItem> tracks_collection = tracks_list.Select(t => t.Track);
                        foreach (IPlayableItem? item in tracks_collection)
                        {
                            if (item is not null and FullTrack track)
                            {
                                tracks.Add(new SpotifyTrackInfo(track, playlist));
                            }
                        }
                    }

                    return tracks;
                }
            }

            {
                string? album_id = SpotifyQueryDecomposer.TryGetAlbumId(query);
                if (!string.IsNullOrWhiteSpace(album_id))
                {
                    FromAlbumId(album_id, tracks);
                }
            }

            {
                string? artist_id = SpotifyQueryDecomposer.TryGetArtistId(query);
                if (!string.IsNullOrWhiteSpace(artist_id))
                {
                    Paging<SimpleAlbum> albums = Artists.GetAlbums(artist_id).GetAwaiter().GetResult();
                    if (albums == null || albums.Items == null || !albums.Items.Any())
                    {
                        return tracks;
                    }

                    foreach (SimpleAlbum album in albums.Items)
                    {
                        FromAlbumId(album.Id, tracks);
                    }

                    return tracks;
                }
            }

            {
                string? track_id = SpotifyQueryDecomposer.TryGetTrackId(query);
                if (!string.IsNullOrWhiteSpace(track_id))
                {
                    FullTrack? track = Tracks.Get(track_id).GetAwaiter().GetResult();

                    if (track != null)
                    {
                        tracks.Add(new SpotifyTrackInfo(track));
                    }

                    return tracks;
                }
            }

            return tracks;
        }

        private static void FromAlbumId(string album_id, List<SpotifyTrackInfo> tracks)
        {
            FullAlbum? album = Albums.Get(album_id).GetAwaiter().GetResult();
            if (album == null)
            {
                return;
            }
            SimpleAlbum simpleAlbum = new()
            {
                Artists = album.Artists,
                AlbumType = album.AlbumType,
                AvailableMarkets = album.AvailableMarkets,
                ExternalUrls = album.ExternalUrls,
                Href = album.Href,
                Id = album.Id,
                Images = album.Images,
                Name = album.Name,
                ReleaseDate = album.ReleaseDate,
                ReleaseDatePrecision = album.ReleaseDatePrecision,
                Restrictions = album.Restrictions,
                TotalTracks = album.TotalTracks,
                Type = album.Type,
                Uri = album.Uri,
            };
            List<SimpleTrack>? tracks_list = album.Tracks?.Items ?? null;
            if (tracks_list == null || tracks_list.Any())
            {
                return;
            }
            foreach (SimpleTrack track in tracks_list)
            {
                FullTrack full = new()
                {
                    Album = simpleAlbum,
                    Artists = track.Artists,
                    AvailableMarkets = track.AvailableMarkets,
                    DiscNumber = track.DiscNumber,
                    DurationMs = track.DurationMs,
                    Explicit = track.Explicit,
                    ExternalUrls = track.ExternalUrls,
                    Href = track.Href,
                    Id = track.Id,
                    IsPlayable = track.IsPlayable,
                    Name = track.Name,
                    TrackNumber = track.TrackNumber,
                    Type = track.Type,
                    Uri = track.Uri
                };
                tracks.Add(new SpotifyTrackInfo(full));
            }
        }
    }
}
