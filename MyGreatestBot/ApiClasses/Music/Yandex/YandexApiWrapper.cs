using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex API wpapper class
    /// </summary>
    [SupportedOSPlatform("windows")]
    public static class YandexApiWrapper
    {
        [AllowNull]
        private static YandexMusicClient _client;
        private static readonly YandexApiException GenericException = new();

        private static YandexMusicClient Api => _client ?? throw GenericException;

        private static class YandexQueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex TRACK_RE = new("/track/(\\d+)$");
            private static readonly Regex ALBUM_RE = new("/album/(\\d+)$");
            private static readonly Regex ARTIST_RE = new("/artist/(\\d+)$");
            private static readonly Regex PLAYLIST_RE = new("/users/([^/]+)/playlists/(\\d+)$");
#pragma warning restore SYSLIB1045

            internal static string? TryGetTrackId(string query)
            {
                return TRACK_RE.GetMatchValue(query);
            }

            internal static string? TryGetAlbumId(string query)
            {
                return ALBUM_RE.GetMatchValue(query);
            }

            internal static string? TryGetArtistId(string query)
            {
                return ARTIST_RE.GetMatchValue(query);
            }

            internal static (string? user, string? kind) TryGetPlaylistId(string query)
            {
                string?[] strings = PLAYLIST_RE.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }
        }

        public static void PerformAuth()
        {
            YandexCredentialsJSON yandexCredStruct = ConfigManager.GetYandexCredentialsJSON();

            _client = new();

            try
            {
                _ = Api.CreateAuthSession(yandexCredStruct.Username);
                _ = Api.AuthorizeByAppPassword(yandexCredStruct.Password);
            }
            catch (Exception ex)
            {
                _client = null;
                throw new YandexApiException("Cannot authorize", ex);
            }

            if (!Api.IsAuthorized)
            {
                _client = null;
                throw GenericException;
            }

            try
            {
                string token = Api.GetAccessToken().AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    _client = null;
                    throw new ArgumentNullException(nameof(token));
                }
            }
            catch (Exception ex)
            {
                _client = null;
                throw new YandexApiException("Cannot get valid access token", ex);
            }
        }

        public static void Logout()
        {
            _client = null;
        }

        /// <summary>
        /// Search similar track on Yandex
        /// </summary>
        /// <param name="otherTrack">Track info from another API</param>
        /// <returns>Track info</returns>
        public static ITrackInfo? SearchTrack(ITrackInfo otherTrack)
        {
            if (otherTrack.TrackType == ApiIntents.Yandex)
            {
                return otherTrack;
            }

            global::Yandex.Music.Api.Models.Search.YSearch? response = null;
            string last_request = string.Empty;

            if (otherTrack.AlbumName != null)
            {
                last_request = $"{otherTrack.Title} - {otherTrack.AlbumName.Title}";
                response = Api.Search(last_request, global::Yandex.Music.Api.Models.Common.YSearchType.Track);
            }

            if (response == null)
            {
                return null;
            }
            if (response.Tracks == null)
            {
                last_request = $"{otherTrack.Title} - {string.Join(", ", otherTrack.ArtistArr.Select(a => a.Title.ToTransletters()))}";
                response = Api.Search(last_request, global::Yandex.Music.Api.Models.Common.YSearchType.Track);
            }

            if (response == null || response.Tracks == null)
            {
                return null;
            }

            IEnumerable<YSearchTrackModel> tracks = response.Tracks.Results.Where(t => t is not null
                && otherTrack.AlbumName != null
                && !string.IsNullOrWhiteSpace(otherTrack.AlbumName.Title)
                && t.Albums.Select(a => a.Title.ToTransletters().ToUpperInvariant())
                           .Contains(otherTrack.AlbumName.Title.ToTransletters().ToUpperInvariant()));

            if (!tracks.Any())
            {
                last_request = $"{otherTrack.Title} - {string.Join(", ", otherTrack.ArtistArr.Select(a => a.Title.ToTransletters()))}";
                response = Api.Search(last_request, global::Yandex.Music.Api.Models.Common.YSearchType.Track);
                if (response == null || response.Tracks == null)
                {
                    return null;
                }
                tracks = response.Tracks.Results.Where(t => t is not null
                && otherTrack.AlbumName != null
                && !string.IsNullOrWhiteSpace(otherTrack.AlbumName.Title)
                && t.Albums.Select(a => a.Title.ToTransletters().ToUpperInvariant())
                           .Contains(otherTrack.AlbumName.Title.ToTransletters().ToUpperInvariant()));
                if (!tracks.Any())
                {
                    return null;
                }
            }

            YSearchTrackModel t = tracks.First();
            YTrack y = t;
            y.Albums = t.Albums.Select(a => a as YAlbum).ToList();

            ITrackInfo first = new YandexTrackInfo(y, null, true);
            first.ObtainAudioURL();
            return first;
        }

        public static IEnumerable<YandexTrackInfo> GetTracks(string? query)
        {
            List<YandexTrackInfo> tracks_collection = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks_collection;
            }

            {
                string? track_id_str = YandexQueryDecomposer.TryGetTrackId(query);
                if (!string.IsNullOrWhiteSpace(track_id_str))
                {
                    YandexTrackInfo? track = GetTrack(track_id_str);
                    if (track != null)
                    {
                        tracks_collection.Add(track);
                    }
                    return tracks_collection;
                }
            }

            {
                string? album_id_str = YandexQueryDecomposer.TryGetAlbumId(query);
                if (!string.IsNullOrWhiteSpace(album_id_str))
                {
                    List<YandexTrackInfo?> tracks = GetAlbum(album_id_str);
                    if (tracks != null && tracks.Any())
                    {
                        foreach (YandexTrackInfo? track in tracks)
                        {
                            if (track != null)
                            {
                                tracks_collection.Add(track);
                            }
                        }
                    }
                    return tracks_collection;
                }
            }

            {
                string? artist_id_str = YandexQueryDecomposer.TryGetArtistId(query);
                if (!string.IsNullOrWhiteSpace(artist_id_str))
                {
                    List<YandexTrackInfo?> tracks = GetArtist(artist_id_str);
                    if (tracks != null && tracks.Any())
                    {
                        foreach (YandexTrackInfo? track in tracks)
                        {
                            if (track != null)
                            {
                                tracks_collection.Add(track);
                            }
                        }
                    }
                    return tracks_collection;
                }
            }
            {
                (string? playlist_user_str, string? playlist_id_str) = YandexQueryDecomposer.TryGetPlaylistId(query);
                if (!string.IsNullOrWhiteSpace(playlist_user_str) && !string.IsNullOrWhiteSpace(playlist_id_str))
                {
                    List<YandexTrackInfo?> tracks = GetPlaylist(playlist_user_str, playlist_id_str);
                    if (tracks != null && tracks.Any())
                    {
                        foreach (YandexTrackInfo? track in tracks)
                        {
                            if (track != null)
                            {
                                tracks_collection.Add(track);
                            }
                        }
                    }
                    return tracks_collection;
                }
            }

            return tracks_collection;
        }

        private static YandexTrackInfo? GetTrack(string? track_id_str)
        {
            if (string.IsNullOrWhiteSpace(track_id_str))
            {
                return null;
            }

            YTrack track = Api.GetTrack(track_id_str);
            if (track == null)
            {
                return null;
            }

            return new(track);
        }

        private static List<YandexTrackInfo?> GetAlbum(string? album_id_str)
        {
            List<YandexTrackInfo?> tracks_collection = new();

            if (string.IsNullOrWhiteSpace(album_id_str))
            {
                return tracks_collection;
            }

            YAlbum album = Api.GetAlbum(album_id_str);

            try
            {
                IEnumerable<YTrack> tracks = album.Volumes.SelectMany(t => t).Where(t => t != null);
                foreach (YTrack track in tracks)
                {
                    tracks_collection.Add(new(track));
                }
            }
            catch
            {
                return tracks_collection;
            }

            return tracks_collection;
        }

        private static List<YandexTrackInfo?> GetArtist(string? artist_id_str)
        {
            List<YandexTrackInfo?> tracks_collection = new();

            if (string.IsNullOrWhiteSpace(artist_id_str))
            {
                return tracks_collection;
            }

            YArtistBriefInfo info = Api.GetArtist(artist_id_str);
            foreach (YAlbum? album in info.Albums.DistinctBy(t => t.Id).OrderByDescending(a => a.ReleaseDate))
            {
                if (album != null)
                {
                    List<YandexTrackInfo?> tracks = GetAlbum(album.Id);
                    foreach (YandexTrackInfo? track in tracks)
                    {
                        if (track != null)
                        {
                            tracks_collection.Add(track);
                        }
                    }
                }
            }

            return tracks_collection;
        }

        private static List<YandexTrackInfo?> GetPlaylist(string playlist_user_str, string playlist_id_str)
        {
            List<YandexTrackInfo?> tracks_collection = new();

            YPlaylist playlist = Api.GetPlaylist(playlist_user_str, playlist_id_str);

            if (playlist == null)
            {
                return tracks_collection;
            }

            IEnumerable<YTrack> tracks;

            try
            {
                tracks = playlist.Tracks.Select(t => t.Track).Where(t => t != null);
            }
            catch
            {
                return tracks_collection;
            }

            foreach (YTrack track in tracks.Where(t => t != null))
            {
                tracks_collection.Add(new(track, playlist));
            }

            return tracks_collection;
        }
    }
}
