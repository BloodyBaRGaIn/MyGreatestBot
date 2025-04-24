global using YandexApiWrapper = MyGreatestBot.ApiClasses.Music.Yandex.YandexApiWrapper;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Yandex.Music.Api.Common.Debug;
using Yandex.Music.Api.Common.Debug.Writer;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Account;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
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
    public sealed partial class YandexApiWrapper : IRadioMusicAPI, IUrlMusicAPI, IApiGenericException
    {
        private YandexMusicClient? _client;
        ApiException IApiGenericException.GenericException { get; } = new YandexApiException();
        private static IApiGenericException GenericExceptionInstance => Instance;

        private YandexMusicClient Client => _client ?? throw GenericExceptionInstance.GenericException;

        private static partial class YandexQueryDecomposer
        {
            private static readonly Regex TrackRegex = GenerateTrackRegex();
            private static readonly Regex AlbumRegex = GenerateAlbumRegex();
            private static readonly Regex ArtistRegex = GenerateArtistRegex();
            private static readonly Regex PlaylistRegex = GeneratePlaylistRegex();
            private static readonly Regex NewFormatPlaylistRegex = GenerateNewFormatPlaylistRegex();
            private static readonly Regex ChartRegex = GenerateChartRegex();

            internal static string? TryGetTrackId(string query)
            {
                return TrackRegex.GetMatchValue(query);
            }

            internal static string? TryGetAlbumId(string query)
            {
                return AlbumRegex.GetMatchValue(query);
            }

            internal static string? TryGetArtistId(string query)
            {
                return ArtistRegex.GetMatchValue(query);
            }

            internal static (string? user, string? kind) TryGetPlaylistId(string query)
            {
                string?[] strings = PlaylistRegex.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }

            internal static string? TryGetNewFormatPlaylistId(string query)
            {
                return NewFormatPlaylistRegex.GetMatchValue(query);
            }

            internal static bool TryGetAsChart(string query)
            {
                return ChartRegex.IsMatch(query);
            }

            [GeneratedRegex("/track/([^/?]+)")]
            private static partial Regex GenerateTrackRegex();

            [GeneratedRegex("/album/([^/?]+)")]
            private static partial Regex GenerateAlbumRegex();

            [GeneratedRegex("/artist/([^/?]+)")]
            private static partial Regex GenerateArtistRegex();

            [GeneratedRegex("/users/([^/?]+)/playlists/([^/?]+)")]
            private static partial Regex GeneratePlaylistRegex();

            [GeneratedRegex("/playlists/([^/?]+)")]
            private static partial Regex GenerateNewFormatPlaylistRegex();

            [GeneratedRegex("/chart")]
            private static partial Regex GenerateChartRegex();
        }

        private YandexApiWrapper()
        {

        }

        public static YandexApiWrapper Instance { get; } = new();
        public static IUrlMusicAPI UrlMusicInstance => Instance;
        public static IRadioMusicAPI RadioMusicInstance => Instance;

        ApiIntents IAPI.ApiType => ApiIntents.Yandex;
        ApiStatus IAPI.OldStatus { get; set; }

        DomainCollection IAccessible.Domains { get; } = "https://music.yandex.ru/";

        void IAPI.PerformAuthInternal()
        {
            YandexCredentialsJSON yandexCredStruct = ConfigManager.GetYandexCredentialsJSON();

            IDebugWriter writer = new DefaultDebugWriter("responses", "log.txt");

            _client = new(new DebugSettings(writer)
            {
                ClearDirectory = true,
                SaveResponse = true
            });

            YAuthTypes types = Client.CreateAuthSession(yandexCredStruct.Username.ToLowerInvariant());

            if (bool.TryParse(types.CanAuthorize, out bool canAuthorize))
            {
                if (!canAuthorize)
                {
                    throw GenericExceptionInstance.GenericException;
                }
            }

            Task.Delay(2000).Wait();

            List<Exception> exceptions = [];

            do
            {
                if (types.AuthMethods.Contains(YAuthMethod.Password))
                {
                    DiscordWrapper.CurrentDomainLogHandler.Send("Trying with password");
                    try
                    {
                        YAuthBase res = Client.AuthorizeByAppPassword(yandexCredStruct.Password);
                        if (res.Errors == null || res.Errors.Count == 0)
                        {
                            break;
                        }
                        exceptions.AddRange(res.Errors.Select(err => new Exception($"Error: {err}")));
                    }
                    catch (Exception ex)
                    {
                        exceptions.Add(ex);
                    }
                }

                types = Client.CreateAuthSession(yandexCredStruct.Username.ToLowerInvariant());

                if (types.AuthMethods.Contains(YAuthMethod.MagicTokenWithPictures))
                {
                    DiscordWrapper.CurrentDomainLogHandler.Send("Trying with captcha");
                    YAuthCaptcha? captcha = Client.GetCaptcha();
                    if (captcha != null)
                    {
                        Console.WriteLine(captcha.ImageUrl);
                        string? answer = Console.ReadLine();
                        if (!string.IsNullOrWhiteSpace(answer))
                        {
                            YAuthBase res = Client.AuthorizeByCaptcha(answer);
                            if (res.Errors == null || res.Errors.Count == 0)
                            {
                                break;
                            }
                            exceptions.AddRange(res.Errors.Select(err => new Exception($"Error: {err}")));
                        }
                    }
                }

                types = Client.CreateAuthSession(yandexCredStruct.Username.ToLowerInvariant());

                if (types.AuthMethods.Contains(YAuthMethod.MagicLink))
                {
                    DiscordWrapper.CurrentDomainLogHandler.Send("Trying with letter");
                    YAuthLetter letter = Client.GetAuthLetter();
                    bool res = false;
                    while (!res)
                    {
                        try
                        {
                            res = Client.AuthorizeByLetter();
                        }
                        catch
                        {
                            try
                            {
                                Task.Delay(1000).Wait();
                            }
                            catch
                            {
                                throw;
                            }
                        }
                    }
                    break;
                }
            }
            while (false);

            if (!Client.IsAuthorized)
            {
                throw GenericExceptionInstance.GenericException;
            }

            try
            {
                string token = Client.GetAccessToken().AccessToken;
                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new ArgumentNullException(nameof(token));
                }
            }
            catch (Exception ex)
            {
                throw new YandexApiException("Cannot get valid access token", ex);
            }
        }

        void IAPI.LogoutInternal()
        {
            _client = null;
        }

        BaseTrackInfo? IRadioMusicAPI.GetRadio(string id)
        {
            id = id.EnsureIdentifier();

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

            BaseTrackInfo track = new YandexTrackInfo(next);
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

        IEnumerable<BaseTrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string? url)
        {
            List<BaseTrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(url) || !IAccessible.IsUrlSuccess(url, false))
            {
                return tracks_collection;
            }

            while (true)
            {
                string track_id_str = YandexQueryDecomposer.TryGetTrackId(url).EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(track_id_str))
                {
                    break;
                }
                BaseTrackInfo? track = UrlMusicInstance.GetTrackFromId(track_id_str);
                if (track != null)
                {
                    tracks_collection.Add(track);
                }
                return tracks_collection;
            }

            while (true)
            {
                string album_id_str = YandexQueryDecomposer.TryGetAlbumId(url).EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(album_id_str))
                {
                    break;
                }
                tracks_collection.AddRange(GetAlbum(album_id_str));
                return tracks_collection;
            }

            while (true)
            {
                string artist_id_str = YandexQueryDecomposer.TryGetArtistId(url).EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(artist_id_str))
                {
                    break;
                }
                tracks_collection.AddRange(GetArtist(artist_id_str));
                return tracks_collection;
            }

            while (true)
            {
                (string? playlist_user_str, string? playlist_id_str) = YandexQueryDecomposer.TryGetPlaylistId(url);
                playlist_user_str = playlist_user_str.EnsureIdentifier();
                playlist_id_str = playlist_id_str.EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(playlist_user_str) || string.IsNullOrWhiteSpace(playlist_id_str))
                {
                    break;
                }
                tracks_collection.AddRange(GetPlaylist(playlist_user_str, playlist_id_str));
                return tracks_collection;
            }

            while (true)
            {
                string? playlist_id_str = YandexQueryDecomposer.TryGetNewFormatPlaylistId(url);
                playlist_id_str = playlist_id_str.EnsureIdentifier();
                if (string.IsNullOrWhiteSpace(playlist_id_str))
                {
                    break;
                }
                tracks_collection.AddRange(GetPlaylist(playlist_id_str));
                return tracks_collection;
            }

            while (true)
            {
                if (!YandexQueryDecomposer.TryGetAsChart(url))
                {
                    break;
                }

                tracks_collection.AddRange(GetChart());

                return tracks_collection;
            }

            return null;
        }

        BaseTrackInfo? IMusicAPI.GetTrackFromId(string? track_id_str, int time)
        {
            track_id_str = track_id_str.EnsureIdentifier();

            if (string.IsNullOrWhiteSpace(track_id_str))
            {
                return null;
            }

            YTrack origin = Client.GetTrack(track_id_str);
            BaseTrackInfo track = new YandexTrackInfo(origin);
            if (time > 0)
            {
                track.PerformRewind(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        #region Private methods

        private List<YandexTrackInfo> GetAlbum(string? album_id_str)
        {
            album_id_str = album_id_str.EnsureIdentifier();

            List<YandexTrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(album_id_str))
            {
                return tracks_collection;
            }

            YAlbum album = Client.GetAlbum(album_id_str);
            IEnumerable<YTrack> volumes;

            try
            {
                volumes = album.WithTracks().Volumes.SelectMany(static t => t)
                               .Where(static t => t != null && !string.IsNullOrWhiteSpace(t.Id))
                               .DistinctBy(static t => t.Id);
            }
            catch
            {
                throw;
            }

            if (!volumes.Any())
            {
                throw new YandexApiException("Album is empty");
            }

            tracks_collection.AddRange(volumes
                .Select(static t => new YandexTrackInfo(t))
                .Where(static t => t != null));

            return tracks_collection;
        }

        private List<YandexTrackInfo> GetArtist(string? artist_id_str)
        {
            artist_id_str = artist_id_str.EnsureIdentifier();

            List<YandexTrackInfo> tracks_collection = [];

            if (string.IsNullOrWhiteSpace(artist_id_str))
            {
                return tracks_collection;
            }

            YArtist artist = Client.GetArtist(artist_id_str).Artist;
            IEnumerable<YTrack> yTracks;

            try
            {
                yTracks = Client.GetArtist(artist_id_str).Artist
                    .GetAllTracks()
                    .Where(static t => t != null);
            }
            catch
            {
                throw;
            }

            if (!yTracks.Any())
            {
                throw new YandexApiException("Artist is empty");
            }

            tracks_collection.AddRange(yTracks
                .Select(static t => new YandexTrackInfo(t))
                .Where(static t => t != null));

            return tracks_collection;
        }

        private static List<YandexTrackInfo> MakePlaylist(YPlaylist playlist)
        {
            List<YandexTrackInfo> tracks_collection = [];

            if (playlist == null)
            {
                return tracks_collection;
            }

            if (playlist.TrackCount < 0)
            {
                throw new YandexApiException("Playlist is empty");
            }

            IEnumerable<YTrack> tracks;

            try
            {
                tracks = playlist.Tracks
                    .Select(static t => t.Track)
                    .Where(static t => t != null);
            }
            catch
            {
                throw;
            }

            try
            {
                tracks_collection.AddRange(tracks.Where(static t => t != null)
                                                 .Select(t => new YandexTrackInfo(t, playlist))
                                                 .Where(static t => t != null));
            }
            catch
            {
                throw;
            }

            return tracks_collection;
        }

        private List<YandexTrackInfo> GetPlaylist(string playlist_user_str, string playlist_id_str)
        {
            YPlaylist playlist;

            try
            {
                playlist = Client.GetPlaylist(playlist_user_str, playlist_id_str);
                return MakePlaylist(playlist);
            }
            catch (Exception ex)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
                throw;
            }
        }

        private List<YandexTrackInfo> GetPlaylist(string playlist_id_str)
        {
            YPlaylist playlist;

            try
            {
                playlist = Client.GetPlaylist(playlist_id_str);
                return MakePlaylist(playlist);
            }
            catch (Exception ex)
            {
                DiscordWrapper.CurrentDomainLogErrorHandler.Send(ex.GetExtendedMessage());
                throw;
            }
        }

        private List<YandexTrackInfo> GetChart()
        {
            List<YandexTrackInfo> tracks_collection = [];

            List<YLandingBlock> blocks = Client.GetLanding(YLandingBlockType.Chart).Blocks;

            if (blocks == null || blocks.Count == 0)
            {
                return tracks_collection;
            }

            IEnumerable<YLandingEntity> entities = blocks.SelectMany(static b => b.Entities);

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
