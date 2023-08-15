using DicordNET.ApiClasses.Spotify;
using DicordNET.Config;
using DicordNET.Extensions;
using System.Text.RegularExpressions;
using Yandex.Music.Api;
using Yandex.Music.Api.Common;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;

namespace DicordNET.ApiClasses.Yandex
{
    internal static class YandexApiWrapper
    {
        /// <summary>
        /// Yandex API wpapper class
        /// </summary>
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

        private static YandexMusicApi? api;
        private static AuthStorage? storage;

        internal static void PerformAuth()
        {
            YandexCredentialsJSON yandexCredStruct = ConfigManager.GetYandexCredentialsJSON();

            api = new();
            storage = new();

            try
            {
                _ = api.User.CreateAuthSession(storage, yandexCredStruct.Username);
            }
            catch
            {
                throw new InvalidOperationException("Cannot create auth session");
            }

            try
            {
                _ = api.User.AuthorizeByAppPassword(storage, yandexCredStruct.Password);
            }
            catch
            {
                throw new InvalidOperationException("Cannot authorize");
            }

            try
            {
                string token = api.User.GetAccessToken(storage).AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentNullException(nameof(token));
                }
            }
            catch
            {
                throw new InvalidOperationException("Cannot get valid token");
            }
        }

        internal static void Logout()
        {

        }

        internal static ITrackInfo? Search(SpotifyTrackInfo spotifyTrack)
        {
            if (api == null)
            {
                return null;
            }

            global::Yandex.Music.Api.Models.Search.YSearch? response = null;
            string last_request = string.Empty;

            if (spotifyTrack.AlbumName != null)
            {
                last_request = $"{spotifyTrack.Title} - {spotifyTrack.AlbumName.Title}";
                response = api?.Search.Track(storage, last_request).Result;
            }

            if (response == null)
            {
                return null;
            }
            if (response.Tracks == null)
            {
                last_request = $"{spotifyTrack.Title} - {string.Join(", ", spotifyTrack.ArtistArr.Select(a => a.Title.ToTransletters()))}";
                response = api?.Search.Track(storage, last_request).Result;
            }

            if (response == null || response.Tracks == null)
            {
                return null;
            }

            IEnumerable<YSearchTrackModel> tracks = response.Tracks.Results.Where(t => t is not null
                && spotifyTrack.AlbumName != null
                && !string.IsNullOrWhiteSpace(spotifyTrack.AlbumName.Title)
                && t.Albums.Select(a => a.Title.ToTransletters().ToUpperInvariant())
                           .Contains(spotifyTrack.AlbumName.Title.ToTransletters().ToUpperInvariant()));

            if (!tracks.Any())
            {
                return null;
            }

            var t = tracks.First();
            YTrack y = t;
            y.Albums = t.Albums.Select(a => a as YAlbum).ToList();

            ITrackInfo first = new YandexTrackInfo(y, null, true);
            first.ObtainAudioURL();
            return first;
        }

        internal static List<YandexTrackInfo> GetTracks(string? query)
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

        internal static string GetAudioURL(string track_id)
        {
            if (api == null)
            {
                return string.Empty;
            }

            try
            {
                return api.Track.GetFileLinkAsync(storage, track_id).GetAwaiter().GetResult();
            }
            catch
            {
                return string.Empty;
            }
        }

        private static YandexTrackInfo? GetTrack(string? track_id_str)
        {
            if (api == null || string.IsNullOrWhiteSpace(track_id_str))
            {
                return null;
            }

            List<YTrack> tracks = api.Track.GetAsync(storage, track_id_str).GetAwaiter().GetResult().Result;
            if (tracks.Count == 0)
            {
                return null;
            }

            YTrack track = tracks[0];

            if (track == null)
            {
                return null;
            }

            return new(track);
        }

        private static List<YandexTrackInfo?> GetAlbum(string? album_id_str)
        {
            List<YandexTrackInfo?> tracks_collection = new();

            if (string.IsNullOrWhiteSpace(album_id_str) || api == null)
            {
                return tracks_collection;
            }

            YAlbum album = api.Album.GetAsync(storage, album_id_str).GetAwaiter().GetResult().Result;

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

            if (string.IsNullOrWhiteSpace(artist_id_str) || api == null)
            {
                return tracks_collection;
            }

            YArtistBriefInfo info = api.Artist.GetAsync(storage, artist_id_str).GetAwaiter().GetResult().Result;
            foreach (YAlbum? album in info.Albums.Concat(info.AlsoAlbums).DistinctBy(t => t.Id).OrderByDescending(a => a.ReleaseDate))
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

            if (api == null)
            {
                return tracks_collection;
            }

            YPlaylist playlist = api.Playlist.GetAsync(storage, playlist_user_str, playlist_id_str)
                                             .GetAwaiter()
                                             .GetResult().Result;

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

            foreach (YTrack track in tracks)
            {
                if (track != null)
                {
                    tracks_collection.Add(new(track, playlist));
                }
            }

            return tracks_collection;
        }
    }
}
