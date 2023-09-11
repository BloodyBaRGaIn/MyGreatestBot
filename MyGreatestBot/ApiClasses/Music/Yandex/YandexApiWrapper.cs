using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using Yandex.Music.Api.Common.Debug;
using Yandex.Music.Api.Common.Debug.Writer;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Search;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    public sealed class YandexApiWrapper : IMusicAPI, ISearchable
    {
        [AllowNull]
        private YandexMusicClient _client;
        private readonly YandexApiException GenericException = new();

        private YandexMusicClient Api => _client ?? throw GenericException;

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

        private YandexApiWrapper()
        {

        }

        public static IMusicAPI Instance { get; private set; } = new YandexApiWrapper();

        ApiIntents IAPI.ApiType => ApiIntents.Yandex;

        DomainCollection IAccessible.Domains { get; } = new("https://music.yandex.ru/");

        public void PerformAuth()
        {
            YandexCredentialsJSON yandexCredStruct = ConfigManager.GetYandexCredentialsJSON();

            IDebugWriter writer = new DefaultDebugWriter("responses", "log.txt");

            _client = new(new DebugSettings(writer)
            {
                ClearDirectory = true,
                SaveResponse = true
            });

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

        public void Logout()
        {
            _client = null;
        }

        public ITrackInfo? SearchTrack(ITrackInfo other)
        {
            if (other.TrackType == ApiIntents.Yandex)
            {
                return other;
            }

            string last_request = $"{other.Title} - {string.Join(", ", other.ArtistArr.Select(a => a.Title.ToTransletters()))}";
            YSearch? response = Api.Search(last_request, YSearchType.Track);

            if (response == null || response.Tracks == null)
            {
                return null;
            }

            IEnumerable<YSearchTrackModel> tracks = response.Tracks.Results.Where(t => t != null
                && t.Artists.FirstOrDefault()?.Name.ToTransletters().ToUpperInvariant()
                    == other.ArtistArr.FirstOrDefault()?.Title.ToTransletters().ToUpperInvariant());

            YSearchTrackModel t = tracks.First();
            YTrack y = t;
            y.Albums = t.Albums.Select(a => a as YAlbum).ToList();

            ITrackInfo first = new YandexTrackInfo(y, null, true);
            first.ObtainAudioURL();
            return first;
        }

        public IEnumerable<ITrackInfo> GetTracks(string? query)
        {
            List<ITrackInfo> tracks_collection = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks_collection;
            }

            {
                string? track_id_str = YandexQueryDecomposer.TryGetTrackId(query);
                if (!string.IsNullOrWhiteSpace(track_id_str))
                {
                    ITrackInfo? track = GetTrack(track_id_str);
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

        public ITrackInfo? GetTrack(string? track_id_str)
        {
            if (string.IsNullOrWhiteSpace(track_id_str))
            {
                return null;
            }

            YTrack track = Api.GetTrack(track_id_str);

            return track == null ? null : new YandexTrackInfo(track);
        }

        private List<YandexTrackInfo?> GetAlbum(string? album_id_str)
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

        private List<YandexTrackInfo?> GetArtist(string? artist_id_str)
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

        private List<YandexTrackInfo?> GetPlaylist(string playlist_user_str, string playlist_id_str)
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
