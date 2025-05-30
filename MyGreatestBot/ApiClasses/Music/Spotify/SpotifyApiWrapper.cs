﻿global using SpotifyApiWrapper = MyGreatestBot.ApiClasses.Music.Spotify.SpotifyApiWrapper;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
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
    public sealed partial class SpotifyApiWrapper : IUrlMusicAPI, IApiGenericException
    {
        private SpotifyClient? _api;

        ApiException IApiGenericException.GenericException { get; } = new SpotifyApiException();
        private static IApiGenericException GenericExceptionInstance => Instance;

        private IPlaylistsClient Playlists => _api?.Playlists ?? throw GenericExceptionInstance.GenericException;
        private IAlbumsClient Albums => _api?.Albums ?? throw GenericExceptionInstance.GenericException;
        private IArtistsClient Artists => _api?.Artists ?? throw GenericExceptionInstance.GenericException;
        private ITracksClient Tracks => _api?.Tracks ?? throw GenericExceptionInstance.GenericException;

        private static partial class SpotifyQueryDecomposer
        {
            private static readonly Regex PlaylistRegex = GeneratePlaylistRegex();
            private static readonly Regex AlbumRegex = GenerateAlbumRegex();
            private static readonly Regex ArtistRegex = GenerateArtistRegex();
            private static readonly Regex TrackRegex = GenerateTrackRegex();

            internal static string? TryGetPlaylistId(string query)
            {
                return PlaylistRegex.GetMatchValue(query);
            }

            internal static string? TryGetAlbumId(string query)
            {
                return AlbumRegex.GetMatchValue(query);
            }

            internal static string? TryGetArtistId(string query)
            {
                return ArtistRegex.GetMatchValue(query);
            }

            internal static string? TryGetTrackId(string query, int time = 0)
            {
                _ = time;
                return TrackRegex.GetMatchValue(query);
            }

            [GeneratedRegex("/playlist/([^/?]+)")]
            private static partial Regex GeneratePlaylistRegex();

            [GeneratedRegex("/album/([^/?]+)")]
            private static partial Regex GenerateAlbumRegex();

            [GeneratedRegex("/artist/([^/?]+)")]
            private static partial Regex GenerateArtistRegex();

            [GeneratedRegex("/track/([^/?]+)")]
            private static partial Regex GenerateTrackRegex();
        }

        private SpotifyApiWrapper()
        {

        }

        public static SpotifyApiWrapper Instance { get; } = new();
        public static IUrlMusicAPI UrlMusicInstance => Instance;

        ApiIntents IAPI.ApiType => ApiIntents.Spotify;
        ApiStatus IAPI.OldStatus { get; set; }

        DomainCollection IAccessible.Domains { get; } = new("https://open.spotify.com/", string.Empty);

        void IAPI.PerformAuthInternal()
        {
            SpotifyCredentialsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault().WithAuthenticator(
                new ClientCredentialsAuthenticator(
                    spotifyClientSecrets.ClientId,
                    spotifyClientSecrets.ClientSecret));

            _api = new(config);
        }

        void IAPI.LogoutInternal()
        {
            _api = null;
        }

        IEnumerable<BaseTrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string? url)
        {
            List<BaseTrackInfo> tracks = [];

            if (string.IsNullOrWhiteSpace(url) || !IAccessible.IsUrlSuccess(url, false))
            {
                return tracks;
            }

            if (_api == null)
            {
                throw GenericExceptionInstance.GenericException;
            }

            while (true)
            {
                string playlist_id = SpotifyQueryDecomposer.TryGetPlaylistId(url).EnsureIdentifier();

                if (string.IsNullOrWhiteSpace(playlist_id))
                {
                    break;
                }

                FullPlaylist playlist = Playlists.Get(playlist_id).GetAwaiter().GetResult() ??
                    throw new SpotifyApiException("Cannot get playlist");

                List<PlaylistTrack<IPlayableItem>> tracks_list = playlist.Tracks?.Items ??
                    throw new SpotifyApiException("Cannot get playlist items");

                if (tracks_list.Count == 0)
                {
                    throw new SpotifyApiException("Playlist is empty");
                }

                IEnumerable<IPlayableItem> tracks_collection = tracks_list.Select(static t => t.Track);

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
                string album_id = SpotifyQueryDecomposer.TryGetAlbumId(url).EnsureIdentifier();

                if (string.IsNullOrWhiteSpace(album_id))
                {
                    break;
                }

                FromAlbumId(album_id, tracks);
                return tracks;
            }

            while (true)
            {
                string artist_id = SpotifyQueryDecomposer.TryGetArtistId(url).EnsureIdentifier();

                if (string.IsNullOrWhiteSpace(artist_id))
                {
                    break;
                }

                Paging<SimpleAlbum> paged_albums = Artists.GetAlbums(artist_id).GetAwaiter().GetResult() ??
                    throw new SpotifyApiException("Cannot get artist");

                List<SimpleAlbum> albums = paged_albums.Items ??
                    throw new SpotifyApiException("Cannot get artist albums");

                if (albums.Count == 0)
                {
                    throw new SpotifyApiException("Artist is empty");
                }

                foreach (SimpleAlbum album in paged_albums.Items)
                {
                    FromAlbumId(album.Id, tracks);
                }

                return tracks;
            }

            while (true)
            {
                string track_id = SpotifyQueryDecomposer.TryGetTrackId(url).EnsureIdentifier();

                if (string.IsNullOrWhiteSpace(track_id))
                {
                    break;
                }

                BaseTrackInfo? track = UrlMusicInstance.GetTrackFromId(track_id);

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
            FullTrack? origin = Tracks.Get(id).GetAwaiter().GetResult();
            BaseTrackInfo track = new SpotifyTrackInfo(origin);
            if (time > 0)
            {
                track.PerformRewind(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        #region Private methods

        private void FromAlbumId(string album_id, List<BaseTrackInfo> tracks)
        {
            album_id = album_id.EnsureIdentifier();
            FullAlbum album = Albums.Get(album_id).GetAwaiter().GetResult() ??
                throw new SpotifyApiException("Cannot get album");

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

            List<SimpleTrack> tracks_list = album.Tracks?.Items ??
                throw new SpotifyApiException("Cannot get album items");

            if (tracks_list.Count == 0)
            {
                throw new SpotifyApiException("Album is empty");
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
