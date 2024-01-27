using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    public sealed class VkApiWrapper : IMusicAPI
    {
        [AllowNull]
        private IVkApi _api;
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

        public static IMusicAPI Instance { get; private set; } = new VkApiWrapper();

        ApiIntents IAPI.ApiType => ApiIntents.Vk;

        DomainCollection IAccessible.Domains { get; } = new("https://www.vk.com/");

        public void PerformAuth()
        {
            VkCredentialsJSON credentials = ConfigManager.GetVkCredentialsJSON();

            ServiceCollection serviceCollection = new();
            _ = serviceCollection.AddAudioBypass();
            _api = new VkApi(serviceCollection);

            try
            {
                Logout();

                _api.Authorize(new ApiAuthParams()
                {
                    Login = credentials.Username,
                    Password = credentials.Password
                });
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

        public void Logout()
        {
            _api?.LogOut();
        }

        public IEnumerable<ITrackInfo>? GetTracks(string? query)
        {
            if (_api == null || !_api.IsAuthorized)
            {
                throw GenericException;
            }

            List<ITrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            if (TryAddAsCollection(query, tracks, is_playlist: true))
            {
                return tracks;
            }

            if (TryAddAsCollection(query, tracks, is_playlist: false))
            {
                return tracks;
            }

            return null;
        }

        public ITrackInfo? GetTrack(string id, int time = 0)
        {
            _ = time;
            if (!long.TryParse(id, out long trackId))
            {
                return null;
            }
            VkCollection<Audio> collection = Audio.Get(new AudioGetParams() { AudioIds = new[] { trackId }, Count = 1 });
            Audio? audio = collection.FirstOrDefault();
            return audio == null ? null : new VkTrackInfo(audio);
        }

        public IEnumerable<ITrackInfo>? GetTracksSearch(string query)
        {
            return GetTracks(query);
        }

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
    }
}
