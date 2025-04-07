global using VkApiWrapper = MyGreatestBot.ApiClasses.Music.Vk.VkApiWrapper;
using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
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
    public sealed partial class VkApiWrapper : IUrlMusicAPI, IApiGenericException
    {
        private VkApi? _api;
        ApiException IApiGenericException.GenericException { get; } = new VkApiException();
        private static IApiGenericException GenericExceptionInstance => Instance;

        private IAudioCategory Audio => _api?.Audio ?? throw GenericExceptionInstance.GenericException;

        private static partial class VkQueryDecomposer
        {
            private static readonly Regex PlaylistRegex = GeneratePlaylistRegex();
            private static readonly Regex AlbumRegex = GenerateAlbumRegex();

#pragma warning disable IDE0052
            private static readonly Regex ArtistRegex = GenerateArtistRegex();
            private static readonly Regex TrackRegex = GenerateTrackRegex();
#pragma warning restore IDE0052

            internal static (string? album, string? id) TryGetAlbumId(string query)
            {
                string?[] strings = AlbumRegex.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }

            internal static (string? user, string? id) TryGetPlaylistId(string query)
            {
                string?[] strings = PlaylistRegex.GetMatchValue(query, 1, 2);
                return (strings[0], strings[1]);
            }

            [GeneratedRegex("/music/playlist/([-]?[\\d]+)_([-]?[\\d]+)")]
            private static partial Regex GeneratePlaylistRegex();

            [GeneratedRegex("/music/album/([-]?[\\d]+)_([-]?[\\d]+)")]
            private static partial Regex GenerateAlbumRegex();

            [GeneratedRegex("/artist/([\\w\\d\\-._]+)")]
            private static partial Regex GenerateArtistRegex();

            [GeneratedRegex("/audio([-]?[\\d]+)_([-]?[\\d]+)")]
            private static partial Regex GenerateTrackRegex();
        }

        private VkApiWrapper()
        {

        }

        public static VkApiWrapper Instance { get; } = new();
        public static IUrlMusicAPI UrlMusicInstance => Instance;

        ApiIntents IAPI.ApiType => ApiIntents.Vk;
        ApiStatus IAPI.OldStatus { get; set; }

        DomainCollection IAccessible.Domains { get; } = "https://www.vk.com/";

        void IAPI.PerformAuthInternal()
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

        void IAPI.LogoutInternal()
        {
            _api?.LogOut();
        }

        IEnumerable<BaseTrackInfo>? IUrlMusicAPI.GetTracksFromUrl(string? url)
        {
            if (_api == null || !_api.IsAuthorized)
            {
                throw GenericExceptionInstance.GenericException;
            }

            List<BaseTrackInfo> tracks = [];

            return string.IsNullOrWhiteSpace(url) || !IAccessible.IsUrlSuccess(url, false)
                ? tracks
                : TryAddAsCollection(url, tracks, is_playlist: true)
                ? tracks
                : TryAddAsCollection(url, tracks, is_playlist: false)
                ? tracks
                : null;
        }

        BaseTrackInfo? IMusicAPI.GetTrackFromId(string id, int time)
        {
            id = id.EnsureIdentifier();

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

            BaseTrackInfo track = new VkTrackInfo(origin);
            if (time > 0)
            {
                track.PerformRewind(TimeSpan.FromSeconds(time));
            }

            return track;
        }

        #region Private methods

        private bool TryAddAsCollection(string query, List<BaseTrackInfo> tracks, bool is_playlist)
        {
            bool success = false;

            (string? user, string? id) = is_playlist
                ? VkQueryDecomposer.TryGetPlaylistId(query)
                : VkQueryDecomposer.TryGetAlbumId(query);

            user = user.EnsureIdentifier();
            id = id.EnsureIdentifier();

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

            VkCollection<Audio> vk_tracks = Audio.Get(new AudioGetParams() { OwnerId = user_l, PlaylistId = id_l, }) ??
                throw new VkApiException("Cannot get collection");

            if (!vk_tracks.Any())
            {
                throw new VkApiException("Collection is empty");
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
