using Microsoft.Extensions.DependencyInjection;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    /// <summary>
    /// Vk API wrapper class
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal static class VkApiWrapper
    {
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

        private static IVkApi? api;

        private static readonly VkApiException GenericException = new();

        private static IAudioCategory Audio => api?.Audio ?? throw GenericException;

        internal static void PerformAuth()
        {
            VkCredentialsJSON credentials = ConfigManager.GetVkCredentialsJSON();

            ServiceCollection serviceCollection = new();
            _ = serviceCollection.AddAudioBypass();
            api = new VkApi(serviceCollection);

            try
            {
                Logout();

                api.Authorize(new ApiAuthParams()
                {
                    Login = credentials.Username,
                    Password = credentials.Password
                });
            }
            catch (Exception ex)
            {
                throw new VkApiException("Cannot authorize", ex);
            }

            if (!api.IsAuthorized)
            {
                throw new VkApiException("Cannot authorize");
            }
        }

        internal static void Logout()
        {
            api?.LogOut();
        }

        internal static IEnumerable<VkTrackInfo> GetTracks(string? query)
        {
            if (api == null || !api.IsAuthorized)
            {
                throw GenericException;
            }

            List<VkTrackInfo> tracks = new();

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

            return tracks;
        }

        private static bool TryAddAsCollection(string query, List<VkTrackInfo> tracks, bool is_playlist)
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

            VkNet.Utils.VkCollection<Audio>? vk_tracks = Audio.Get(new AudioGetParams() { OwnerId = user_l, PlaylistId = id_l, });

            if (vk_tracks == null || !vk_tracks.Any())
            {
                return success;
            }

            foreach (Audio t in vk_tracks)
            {
                tracks.Add(new(t, playlist));
            }

            return success;
        }
    }
}
