using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Music.Api.Common.Debug;
using Yandex.Music.Api.Common.Debug.Writer;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Common;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Radio;
using Yandex.Music.Api.Models.Search;
using Yandex.Music.Api.Models.Search.Track;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    public sealed class YandexApiWrapper : IRadioAPI, IMusicAPI, ISearchable
    {
        [AllowNull]
        private YandexMusicClient _client;
        private readonly YandexApiException GenericException = new();

        private YandexMusicClient Client => _client ?? throw GenericException;

        private static class YandexQueryDecomposer
        {
#pragma warning disable SYSLIB1045
            private static readonly Regex TRACK_RE = new("/track/(\\d+)$");
            private static readonly Regex ALBUM_RE = new("/album/(\\d+)$");
            private static readonly Regex ARTIST_RE = new("/artist/(\\d+)$");
            private static readonly Regex PLAYLIST_RE = new("/users/([^/]+)/playlists/([^?]+)");
            private static readonly Regex PODCAST_RE = new("/track/([^?]+)");
#pragma warning restore SYSLIB1045

            internal static string? TryGetPodcastId(string query)
            {
                return PODCAST_RE.GetMatchValue(query);
            }

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

        private static readonly YandexApiWrapper _instance = new();

        public static IMusicAPI MusicInstance { get; } = _instance;
        public static IRadioAPI RadioInstance { get; } = _instance;

        ApiIntents IAPI.ApiType => ApiIntents.Yandex;

        DomainCollection IAccessible.Domains { get; } = "https://music.yandex.ru/";

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
                Task.Delay(500).Wait();
                _ = Client.CreateAuthSession(yandexCredStruct.Username);
                Task.Delay(500).Wait();
                _ = Client.AuthorizeByAppPassword(yandexCredStruct.Password);
            }
            catch (Exception ex)
            {
                Logout();
                throw new YandexApiException("Cannot authorize", ex);
            }

            if (!Client.IsAuthorized)
            {
                Logout();
                throw GenericException;
            }

            try
            {
                string token = Client.GetAccessToken().AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    Logout();
                    throw new ArgumentNullException(nameof(token));
                }
            }
            catch (Exception ex)
            {
                Logout();
                throw new YandexApiException("Cannot get valid access token", ex);
            }
        }

        public void Logout()
        {
            _client = null;
        }

        /// <summary>
        /// Search similar track on Yandex
        /// </summary>
        /// <param name="otherTrack">Track info from another API</param>
        /// <returns>Track info</returns>
        public ITrackInfo? SearchTrack(ITrackInfo otherTrack)
        {
            if (otherTrack.TrackType == ApiIntents.Yandex)
            {
                return otherTrack;
            }

            YSearch? response = null;
            string last_request = string.Empty;

            if (otherTrack.AlbumName != null)
            {
                last_request = $"{otherTrack.Title} - {otherTrack.AlbumName.Title}";
                response = Client.Search(last_request, YSearchType.Track);
            }

            if (response == null)
            {
                return null;
            }
            if (response.Tracks == null)
            {
                last_request = $"{otherTrack.Title} - {string.Join(", ", otherTrack.ArtistArr.Select(a => a.Title.ToTransletters()))}";
                response = Client.Search(last_request, YSearchType.Track);
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
                response = Client.Search(last_request, YSearchType.Track);
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

        public ITrackInfo? GetRadio(string id)
        {
            YTrack originTrack = Client.GetTrack(id);
            if (originTrack == null)
            {
                return null;
            }

            YAlbum? album = originTrack.Albums.FirstOrDefault();
            if (album == null)
            {
                return null;
            }

            List<YStation> stations = Client.GetRadioStations();

            YStation? radio = stations
                .FirstOrDefault(s => string.Equals(
                    s.AdParams.GenreName,
                    album.Genre,
                    StringComparison.OrdinalIgnoreCase));

            if (radio == null)
            {
                return null;
            }

            string temp = string.Empty;

            try
            {
                temp = radio.SendFeedBack(
                    YStationFeedbackType.TrackStarted,
                    originTrack);
            }
            catch { }

            List<YSequenceItem> sequence = radio.GetTracks(id);
            if (sequence == null || sequence.Count == 0)
            {
                return null;
            }

            sequence = sequence.Where(i => !i.Track.Equals(originTrack)).ToList();
            if (sequence.Count == 0)
            {
                return null;
            }

            // still could be duplicates there
            YSequenceItem? item = sequence.Shuffle().FirstOrDefault();
            if (item == null)
            {
                return null;
            }

            YTrack next = item.Track;
            if (next == null)
            {
                return null;
            }

            ITrackInfo track = new YandexTrackInfo(next);
            if (track == null)
            {
                return null;
            }

            try
            {
                temp = radio.SendFeedBack(
                    YStationFeedbackType.TrackFinished,
                    originTrack,
                    "",
                    originTrack.DurationMs / 1000.0);
            }
            catch { }

            _ = temp;

            track.Radio = true;

            return track;
        }

        public IEnumerable<ITrackInfo>? GetTracks(string? query)
        {
            List<ITrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks_collection;
            }

            while (true)
            {
                string? track_id_str = YandexQueryDecomposer.TryGetTrackId(query);
                if (string.IsNullOrWhiteSpace(track_id_str))
                {
                    break;
                }
                ITrackInfo? track = GetTrack(track_id_str);
                if (track != null)
                {
                    tracks_collection.Add(track);
                }
                return tracks_collection;
            }

            while (true)
            {
                string? podcast_id_str = YandexQueryDecomposer.TryGetPodcastId(query);
                if (string.IsNullOrWhiteSpace(podcast_id_str))
                {
                    break;
                }
                ITrackInfo? track = GetTrack(podcast_id_str);
                if (track != null)
                {
                    tracks_collection.Add(track);
                }
                return tracks_collection;
            }

            while (true)
            {
                string? album_id_str = YandexQueryDecomposer.TryGetAlbumId(query);
                if (string.IsNullOrWhiteSpace(album_id_str))
                {
                    break;
                }
                List<YandexTrackInfo?> tracks = GetAlbum(album_id_str);
                if (tracks == null || tracks.Count == 0)
                {
                    return tracks_collection;
                }
                foreach (YandexTrackInfo? track in tracks)
                {
                    if (track != null)
                    {
                        tracks_collection.Add(track);
                    }
                }
                return tracks_collection;
            }

            while (true)
            {
                string? artist_id_str = YandexQueryDecomposer.TryGetArtistId(query);
                if (string.IsNullOrWhiteSpace(artist_id_str))
                {
                    break;
                }
                List<YandexTrackInfo?> tracks = GetArtist(artist_id_str);
                if (tracks == null || tracks.Count == 0)
                {
                    return tracks_collection;
                }
                foreach (YandexTrackInfo? track in tracks)
                {
                    if (track != null)
                    {
                        tracks_collection.Add(track);
                    }
                }
                return tracks_collection;
            }

            while (true)
            {
                (string? playlist_user_str, string? playlist_id_str) = YandexQueryDecomposer.TryGetPlaylistId(query);
                if (string.IsNullOrWhiteSpace(playlist_user_str) || string.IsNullOrWhiteSpace(playlist_id_str))
                {
                    break;
                }
                List<YandexTrackInfo?> tracks = GetPlaylist(playlist_user_str, playlist_id_str);
                if (tracks == null || tracks.Count == 0)
                {
                    return tracks_collection;
                }
                foreach (YandexTrackInfo? track in tracks)
                {
                    if (track != null)
                    {
                        tracks_collection.Add(track);
                    }
                }
                return tracks_collection;
            }

            return null;
        }

        public ITrackInfo? GetTrack(string? track_id_str, int time = 0)
        {
            if (string.IsNullOrWhiteSpace(track_id_str))
            {
                return null;
            }

            YTrack origin = Client.GetTrack(track_id_str);

#pragma warning disable CA1859
            ITrackInfo track = new YandexTrackInfo(origin);
#pragma warning restore CA1859

            if (time > 0)
            {
                track.PerformSeek(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        public IEnumerable<ITrackInfo>? GetTracksSearch(string query)
        {
            return GetTracks(query);
        }

        private List<YandexTrackInfo?> GetAlbum(string? album_id_str)
        {
            List<YandexTrackInfo?> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(album_id_str))
            {
                return tracks_collection;
            }

            YAlbum album = Client.GetAlbum(album_id_str);

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
            List<YandexTrackInfo?> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(artist_id_str))
            {
                return tracks_collection;
            }

            YArtistBriefInfo info = Client.GetArtist(artist_id_str);
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
            List<YandexTrackInfo?> tracks_collection = [];

            YPlaylist playlist = Client.GetPlaylist(playlist_user_str, playlist_id_str);

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
