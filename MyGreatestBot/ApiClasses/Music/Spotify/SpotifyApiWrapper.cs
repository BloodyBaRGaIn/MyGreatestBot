global using SpotifyApiWrapper = MyGreatestBot.ApiClasses.Music.Spotify.SpotifyApiWrapper;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using SpotifyAPI.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    /// <summary>
    /// Spotify API wrapper class
    /// </summary>
    public sealed class SpotifyApiWrapper : IUrlMusicAPI
    {
        private SpotifyClient? _api;
        private readonly SpotifyApiException GenericException = new();

        private IPlaylistsClient Playlists => _api?.Playlists ?? throw GenericException;
        private IAlbumsClient Albums => _api?.Albums ?? throw GenericException;
        private IArtistsClient Artists => _api?.Artists ?? throw GenericException;
        private ITracksClient Tracks => _api?.Tracks ?? throw GenericException;

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

            internal static string? TryGetTrackId(string query, int time = 0)
            {
                _ = time;
                return TRACK_RE.GetMatchValue(query);
            }
        }

        private SpotifyApiWrapper()
        {

        }

        private static readonly SpotifyApiWrapper _instance = new();

        public static IUrlMusicAPI UrlMusicInstance { get; } = _instance;

        ApiIntents IAPI.ApiType => ApiIntents.Spotify;

        DomainCollection IAccessible.Domains { get; } = new("https://open.spotify.com/", string.Empty);

        void IAPI.PerformAuth()
        {
            SpotifyClientSecretsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(
                new ClientCredentialsAuthenticator(
                    spotifyClientSecrets.ClientId,
                    spotifyClientSecrets.ClientSecret));

            _api = new(config);
        }

        void IAPI.Logout()
        {
            _api = null;
        }

        IEnumerable<ITrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string? url)
        {
            List<ITrackInfo> tracks = [];

            if (string.IsNullOrWhiteSpace(url))
            {
                return tracks;
            }

            if (_api == null)
            {
                throw GenericException;
            }

            while (true)
            {
                string? playlist_id = SpotifyQueryDecomposer.TryGetPlaylistId(url);

                if (string.IsNullOrWhiteSpace(playlist_id))
                {
                    break;
                }

                FullPlaylist? playlist = Playlists.Get(playlist_id).GetAwaiter().GetResult();
                List<PlaylistTrack<IPlayableItem>>? tracks_list = playlist.Tracks?.Items ?? null;

                if (tracks_list == null)
                {
                    return tracks;
                }

                IEnumerable<IPlayableItem> tracks_collection = tracks_list.Select(t => t.Track);

                foreach (IPlayableItem? item in tracks_collection)
                {
                    if (item is not null and FullTrack track)
                    {
                        tracks.Add(new SpotifyTrackInfo(track, playlist));
                    }
                }

                return tracks;
            }

            while (true)
            {
                string? album_id = SpotifyQueryDecomposer.TryGetAlbumId(url);

                if (string.IsNullOrWhiteSpace(album_id))
                {
                    break;
                }

                FromAlbumId(album_id, tracks);
                return tracks;
            }

            while (true)
            {
                string? artist_id = SpotifyQueryDecomposer.TryGetArtistId(url);

                if (string.IsNullOrWhiteSpace(artist_id))
                {
                    break;
                }

                Paging<SimpleAlbum> albums = Artists.GetAlbums(artist_id).GetAwaiter().GetResult();
                if (albums == null || albums.Items == null || albums.Items.Count == 0)
                {
                    return tracks;
                }

                foreach (SimpleAlbum album in albums.Items)
                {
                    FromAlbumId(album.Id, tracks);
                }

                return tracks;
            }

            while (true)
            {
                string? track_id = SpotifyQueryDecomposer.TryGetTrackId(url);

                if (string.IsNullOrWhiteSpace(track_id))
                {
                    break;
                }

                ITrackInfo? track = UrlMusicInstance.GetTrack(track_id);

                if (track != null)
                {
                    tracks.Add(track);
                }

                return tracks;
            }

            return null;
        }

        ITrackInfo? IMusicAPI.GetTrack(string id, int time)
        {
            FullTrack? origin = Tracks.Get(id).GetAwaiter().GetResult();

#pragma warning disable CA1859
            ITrackInfo track = new SpotifyTrackInfo(origin);
#pragma warning restore CA1859

            if (time > 0)
            {
                track.PerformSeek(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        #region Private methods

        private void FromAlbumId(string album_id, List<ITrackInfo> tracks)
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
            if (tracks_list == null || tracks_list.Count == 0)
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

        #endregion Private methods
    }
}
