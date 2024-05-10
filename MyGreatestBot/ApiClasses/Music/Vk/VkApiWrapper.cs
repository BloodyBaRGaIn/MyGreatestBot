global using VkApiWrapper = MyGreatestBot.ApiClasses.Music.Vk.VkApiWrapper;
using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;
using VkNet.Utils;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    /// <summary>
    /// Vk API wrapper class
    /// </summary>
    public sealed class VkApiWrapper : IUrlMusicAPI
    {
        private VkApi? _api;
        private readonly VkApiException GenericException = new();

        private IAudioCategory Audio => _api?.Audio ?? throw GenericException;

        private static class VkQueryDecomposer
        {

#pragma warning disable SYSLIB1045
            private static readonly Regex PLAYLIST_RE = new("/music/playlist/([-]?[\\d]+)_([-]?[\\d]+)");
            private static readonly Regex ALBUM_RE = new("/music/album/([-]?[\\d]+)_([-]?[\\d]+)");

#pragma warning disable IDE0052
            private static readonly Regex ARTIST_RE = new("/artist/([\\w\\d\\-._]+)");
            private static readonly Regex TRACK_RE = new("/audio([-]?[\\d]+)_([-]?[\\d]+)");
#pragma warning restore IDE0052

#pragma warning restore SYSLIB1045

            internal static (string? album, string? id) TryGetAlbumId(string query)
            {
                string?[] strings = ALBUM_RE.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }

            internal static (string? user, string? id) TryGetPlaylistId(string query)
            {
                string?[] strings = PLAYLIST_RE.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }
        }

        private VkApiWrapper()
        {

        }

        private static readonly VkApiWrapper _instance = new();

        public static IUrlMusicAPI UrlMusicInstance { get; } = _instance;

        ApiIntents IAPI.ApiType => ApiIntents.Vk;

        DomainCollection IAccessible.Domains { get; } = "https://www.vk.com/";

        void IAPI.PerformAuth()
        {
            VkCredentialsJSON credentials = ConfigManager.GetVkCredentialsJSON();

            ServiceCollection serviceCollection = new();
            _ = serviceCollection.AddAudioBypass();

            try
            {
                _api = new VkApi(serviceCollection);
            }
            catch (Exception ex)
            {
                throw new VkApiException("Cannot add audio service", ex);
            }

            ApiAuthParams apiAuthParams = new()
            {
                Login = credentials.Username,
                Password = credentials.Password
            };

            if (ulong.TryParse(credentials.AppId, out ulong appid))
            {
                apiAuthParams.ApplicationId = appid;
            }

            try
            {
                if (_api == null)
                {
                    throw new ArgumentNullException(nameof(_api), "VkApi is null");
                }
            }
            catch (Exception ex)
            {
                throw new VkApiException("Cannot authorize", ex);
            }

            try
            {
                _api.LogOut();
            }
            catch { }

            try
            {
                _api.Authorize(apiAuthParams);
            }
            catch (Exception ex)
            {
                throw new VkApiException("Cannot authorize", ex);
            }

            if (!_api.IsAuthorized)
            {
                throw new VkApiException("Cannot authorize");
            }
        }

        void IAPI.Logout()
        {
            _api?.LogOut();
        }

        IEnumerable<ITrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string? url)
        {
            if (_api == null || !_api.IsAuthorized)
            {
                throw GenericException;
            }

            List<ITrackInfo> tracks = [];

            return string.IsNullOrWhiteSpace(url)
                ? tracks
                : TryAddAsCollection(url, tracks, is_playlist: true)
                ? tracks
                : TryAddAsCollection(url, tracks, is_playlist: false)
                ? tracks
                : null;
        }

        ITrackInfo? IMusicAPI.GetTrack(string id, int time)
        {
            if (!long.TryParse(id, out long trackId))
            {
                return null;
            }
            VkCollection<Audio> collection = Audio.Get(new AudioGetParams() { AudioIds = [trackId], Count = 1 });
            Audio? origin = collection.FirstOrDefault();
            if (origin == null)
            {
                return null;
            }

#pragma warning disable CA1859
            ITrackInfo track = new VkTrackInfo(origin);
#pragma warning restore CA1859

            if (time > 0)
            {
                track.PerformSeek(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        #region Private methods

        private bool TryAddAsCollection(string query, List<ITrackInfo> tracks, bool is_playlist)
        {
            bool success = false;

            (string? user, string? id) = is_playlist
                ? VkQueryDecomposer.TryGetPlaylistId(query)
                : VkQueryDecomposer.TryGetAlbumId(query);

            if (string.IsNullOrWhiteSpace(user)
                || string.IsNullOrWhiteSpace(id)
                || !long.TryParse(user, out long user_l)
                || !long.TryParse(id, out long id_l))
            {
                return success;
            }

            success = true;

            AudioPlaylist? playlist = is_playlist
                ? Audio.GetPlaylistById(user_l, id_l)
                : null;

            VkCollection<Audio>? vk_tracks = Audio.Get(new AudioGetParams() { OwnerId = user_l, PlaylistId = id_l, });

            if (vk_tracks == null || !vk_tracks.Any())
            {
                return success;
            }

            foreach (Audio t in vk_tracks)
            {
                tracks.Add(new VkTrackInfo(t, playlist));
            }

            return success;
        }

        #endregion Private methods
    }
}
