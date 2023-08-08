﻿using DicordNET.ApiClasses.Extensions;
using DicordNET.Config;
using DicordNET.TrackClasses;
using EmbedIO.Sessions;
using SpotifyAPI.Web;
using SpotifyAPI.Web.Auth;
using SpotifyAPI.Web.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;

namespace DicordNET.ApiClasses
{
    internal static class SpotifyApiWrapper
    {
        private static EmbedIOAuthServer server;
        private static SpotifyClient? SpotifyClientInstance;
        private static nint session_id;
        private static IPlaylistsClient Playlists => SpotifyClientInstance?.Playlists ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        private static IAlbumsClient Albums => SpotifyClientInstance?.Albums ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        private static IArtistsClient Artists => SpotifyClientInstance?.Artists ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        private static ITracksClient Tracks => SpotifyClientInstance?.Tracks ?? throw new ArgumentException(nameof(SpotifyClientInstance));
        internal static IPlayerClient Player => SpotifyClientInstance?.Player ?? throw new ArgumentException(nameof(SpotifyClientInstance));


        private static class SpotifyQueryDecomposer
        {
            //https://open.spotify.com/playlist/6NWEsOPK2E63K8C882OdGW
            private static readonly Regex PLAYLIST_RE = new("/playlist/([a-zA-Z0-9]+)$");
            //https://open.spotify.com/album/05b3WF6jDA6kf7JtIvKw2c
            private static readonly Regex ALBUM_RE = new("/album/([a-zA-Z0-9]+)$");
            //https://open.spotify.com/artist/7iWiAD5LLKyiox2grgfmUT
            private static readonly Regex ARTIST_RE = new("/artist/([a-zA-Z0-9]+)$");
            //https://open.spotify.com/track/1EJzHoU6rg1afMozs9t6aM
            private static readonly Regex TRACK_RE = new("/track/([a-zA-Z0-9]+)$");

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

            server = new(new Uri("http://localhost:5543/callback"), 5543);
            server.Start().Wait();

            server.AuthorizationCodeReceived += Server_AuthorizationCodeReceived;
            server.ErrorReceived += Server_ErrorReceived;

            var request = new LoginRequest(server.BaseUri, spotifyClientSecrets.ClientId, LoginRequest.ResponseType.Code)
            {
                Scope = new List<string> { Scopes.UserModifyPlaybackState, Scopes.UserReadPlaybackState, Scopes.UserReadCurrentlyPlaying, Scopes.UserReadEmail }
            };
            BrowserUtil.Open(request.ToUri());

            //SpotifyClientConfig config = SpotifyClientConfig.CreateDefault()
            //    .WithAuthenticator(
            //        new ClientCredentialsAuthenticator(
            //            spotifyClientSecrets.ClientId,
            //            spotifyClientSecrets.ClientSecret
            //            )
            //        );

            //if (SpotifyClientInstance != null)
            //{
            //    lock (SpotifyClientInstance)
            //    {
            //        SpotifyClientInstance = new(config);
            //    }
            //}
            //else
            //{
            //    SpotifyClientInstance = new(config);
            //}
        }

        private static Task Server_ErrorReceived(object arg1, string arg2, string? arg3)
        {
            throw new NotImplementedException();
        }

        private static async Task Server_AuthorizationCodeReceived(object sender, AuthorizationCodeResponse response)
        {
            await server.Stop();

            SpotifyClientSecretsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

            var config = SpotifyClientConfig.CreateDefault();
            var tokenResponse = await new OAuthClient(config).RequestToken(
              new AuthorizationCodeTokenRequest(
                spotifyClientSecrets.ClientId, spotifyClientSecrets.ClientSecret, response.Code, new Uri("http://localhost:5543/callback")
              )
            );

            SpotifyClientInstance = new(tokenResponse.AccessToken);
        }

        internal static void Logout()
        {

        }

        internal static List<ITrackInfo> GetTracks(string? query)
        {
            List<ITrackInfo> tracks = new();

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
                        var tracks_collection = tracks_list.Select(t => t.Track);
                        foreach (var track in tracks_collection)
                        {
                            tracks.Add(new SpotifyTrackInfo(track, playlist));
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
                    var albums = Artists.GetAlbums(artist_id).GetAwaiter().GetResult();
                    if (albums == null || albums.Items == null || !albums.Items.Any())
                    {
                        return tracks;
                    }

                    foreach (var album in albums.Items)
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

        private static void FromAlbumId(string album_id, List<ITrackInfo> tracks)
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
                AlbumGroup = string.Empty,
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
            if (tracks_list == null)
            {
                return;
            }
            var tracks_collection = tracks_list.Select(t => t);
            foreach (var track in tracks_collection)
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
