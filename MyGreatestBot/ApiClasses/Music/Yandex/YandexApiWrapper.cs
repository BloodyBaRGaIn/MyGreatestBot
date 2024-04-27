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
using Yandex.Music.Api.Models.Landing;
using Yandex.Music.Api.Models.Landing.Entity;
using Yandex.Music.Api.Models.Landing.Entity.Entities;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Radio;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex API wrapper class
    /// </summary>
    public sealed class YandexApiWrapper : IRadioAPI, IMusicAPI
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
            private static readonly Regex ARTIST_RE = new("/artist/(\\d+)(/\\w*)?$");
            private static readonly Regex PLAYLIST_RE = new("/users/([^/]+)/playlists/([^?]+)");
            private static readonly Regex PODCAST_RE = new("/track/([^?]+)");
            private static readonly Regex CHART_RE = new("/chart");
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

            internal static bool TryGetAsChart(string query)
            {
                return CHART_RE.IsMatch(query);
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

        void IAPI.PerformAuth()
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
                (this as IAPI).Logout();
                throw new YandexApiException("Cannot authorize", ex);
            }

            if (!Client.IsAuthorized)
            {
                (this as IAPI).Logout();
                throw GenericException;
            }

            try
            {
                string token = Client.GetAccessToken().AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    (this as IAPI).Logout();
                    throw new ArgumentNullException(nameof(token));
                }
            }
            catch (Exception ex)
            {
                (this as IAPI).Logout();
                throw new YandexApiException("Cannot get valid access token", ex);
            }
        }

        void IAPI.Logout()
        {
            _client = null;
        }

        ITrackInfo? IRadioAPI.GetRadio(string id)
        {
            YTrack originTrack = Client.GetTrack(id);
            if (originTrack == null)
            {
                return null;
            }

            YAlbum? mainAlbum = originTrack.Albums.FirstOrDefault();
            if (mainAlbum == null)
            {
                return null;
            }

            string mainGenre = mainAlbum.Genre;

            List<YStation> stations = Client.GetRadioStations();

            YStation? radio = stations
                .FirstOrDefault(s => string.Equals(
                    s.AdParams.GenreName,
                    mainGenre,
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

            IEnumerable<YSequenceItem> sequence = radio.GetTracks(id);
            if (sequence == null || !sequence.Any())
            {
                return null;
            }

            sequence = sequence.Where(i => !i.Track.Equals(originTrack));
            if (sequence == null || !sequence.Any())
            {
                return null;
            }

            IEnumerable<YSequenceItem> sameGenreTracks = sequence.Where(i => string.Equals(
                (i.Track.Albums.FirstOrDefault() ?? new()).Genre,
                mainGenre,
                StringComparison.OrdinalIgnoreCase));

            YSequenceItem? selectedItem = (sameGenreTracks.Any() ? sameGenreTracks : sequence)
                .Shuffle()
                .FirstOrDefault();

            if (selectedItem == null)
            {
                return null;
            }

            YTrack next = selectedItem.Track;
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
                    originTrack);
            }
            catch { }

            _ = temp;

            track.Radio = true;

            return track;
        }

        IEnumerable<ITrackInfo>? IMusicAPI.GetTracks(string? query)
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
                ITrackInfo? track = MusicInstance.GetTrack(track_id_str);
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
                ITrackInfo? track = MusicInstance.GetTrack(podcast_id_str);
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

                tracks_collection.AddRange(GetAlbum(album_id_str));

                return tracks_collection;
            }

            while (true)
            {
                string? artist_id_str = YandexQueryDecomposer.TryGetArtistId(query);
                if (string.IsNullOrWhiteSpace(artist_id_str))
                {
                    break;
                }

                tracks_collection.AddRange(GetArtist(artist_id_str));

                return tracks_collection;
            }

            while (true)
            {
                (string? playlist_user_str, string? playlist_id_str) = YandexQueryDecomposer.TryGetPlaylistId(query);
                if (string.IsNullOrWhiteSpace(playlist_user_str) || string.IsNullOrWhiteSpace(playlist_id_str))
                {
                    break;
                }

                tracks_collection.AddRange(GetPlaylist(playlist_user_str, playlist_id_str));

                return tracks_collection;
            }

            while (true)
            {
                if (!YandexQueryDecomposer.TryGetAsChart(query))
                {
                    break;
                }

                tracks_collection.AddRange(GetChart());

                return tracks_collection;
            }

            return null;
        }

        ITrackInfo? IMusicAPI.GetTrack(string? track_id_str, int time)
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

        IEnumerable<ITrackInfo>? IMusicAPI.GetTracksFromPlainText(string text)
        {
            _ = text;
            throw new NotImplementedException();
        }

        #region Private methods

        private List<YandexTrackInfo> GetAlbum(string? album_id_str)
        {
            List<YandexTrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(album_id_str))
            {
                return tracks_collection;
            }

            YAlbum album = Client.GetAlbum(album_id_str);
            IEnumerable<YTrack> volumes;

            try
            {
                volumes = album.WithTracks().Volumes.SelectMany(t => t)
                               .Where(t => t != null && !string.IsNullOrWhiteSpace(t.Id))
                               .DistinctBy(t => t.Id);
            }
            catch
            {
                return tracks_collection;
            }

            if (!volumes.Any())
            {
                return tracks_collection;
            }

            tracks_collection.AddRange(volumes
                .Select(track => new YandexTrackInfo(track))
                .Where(track => track != null));

            return tracks_collection;
        }

        private List<YandexTrackInfo> GetArtist(string? artist_id_str)
        {
            List<YandexTrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(artist_id_str))
            {
                return tracks_collection;
            }

            IEnumerable<YTrack> yTracks = Client.GetArtist(artist_id_str).Artist
                .GetAllTracks()
                .Where(track => track != null);

            tracks_collection.AddRange(yTracks
                .Select(track => new YandexTrackInfo(track))
                .Where(track => track != null));

            return tracks_collection;
        }

        private List<YandexTrackInfo> GetPlaylist(string playlist_user_str, string playlist_id_str)
        {
            List<YandexTrackInfo> tracks_collection = [];

            YPlaylist playlist = Client.GetPlaylist(playlist_user_str, playlist_id_str);

            if (playlist == null)
            {
                return tracks_collection;
            }

            IEnumerable<YTrack> tracks;

            try
            {
                tracks = playlist.Tracks
                    .Select(t => t.Track)
                    .Where(t => t != null);
            }
            catch
            {
                return tracks_collection;
            }

            try
            {
                tracks_collection.AddRange(tracks.Where(t => t != null)
                                                 .Select(t => new YandexTrackInfo(t, playlist))
                                                 .Where(t => t != null));
            }
            catch
            {
                return tracks_collection;
            }

            return tracks_collection;
        }

        private List<YandexTrackInfo> GetChart()
        {
            List<YandexTrackInfo> tracks_collection = [];

            List<YLandingBlock> blocks = Client.GetLanding(YLandingBlockType.Chart).Blocks;

            if (blocks == null || blocks.Count == 0)
            {
                return tracks_collection;
            }

            IEnumerable<YLandingEntity> entities = blocks.SelectMany(b => b.Entities);

            if (entities == null || !entities.Any())
            {
                return tracks_collection;
            }

            foreach (YLandingEntity entity in entities)
            {
                if (entity is not YLandingEntityChart chart_entity)
                {
                    continue;
                }
                YChartItem data = chart_entity.Data;
                if (data == null)
                {
                    continue;
                }
                YTrack origin = data.Track;
                if (origin == null)
                {
                    continue;
                }
                origin = Client.GetTrack(origin.Id);
                if (origin == null)
                {
                    continue;
                }
                YandexTrackInfo track = new(origin);
                if (track == null)
                {
                    continue;
                }
                tracks_collection.Add(track);
            }

            return tracks_collection;
        }

        #endregion Private methods
    }
}
