using DicordNET.Config;
using DicordNET.TrackClasses;
using Microsoft.Extensions.DependencyInjection;
using System.Text.RegularExpressions;
using VkNet;
using VkNet.Abstractions;
using VkNet.AudioBypassService.Extensions;
using VkNet.Model;

namespace DicordNET.ApiClasses
{
    internal static class VkApiWrapper
    {
        private static class VkQueryDecomposer
        {
            private static readonly Regex PLAYLIST_RE = new("playlist/([a-zA-Z0-9]+)_([a-zA-Z0-9]+)");
            //https://vk.com/music/album/-2000294413_7294413?act=album
            private static readonly Regex ALBUM_RE = new("album/([-]?[\\d]+)_([-]?[\\d]+)");

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

        internal static void Init()
        {
            VkCredentialsJSON credentials = ConfigManager.GetVkCredentialsJSON();

            var serviceCollection = new ServiceCollection();
            serviceCollection.AddAudioBypass();
            api = new VkApi(serviceCollection);

            try
            {
                api.LogOut();

                api.Authorize(new ApiAuthParams()
                {
                    Login = credentials.Username,
                    Password = credentials.Password
                });

                if (!api.IsAuthorized)
                {
                    throw new InvalidOperationException("Cannot authorize");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                Environment.Exit(1);
            }
        }

        internal static void Logout()
        {
            api?.LogOut();
        }

        internal static List<VkTrackInfo> GetTracks(string? query)
        {
            List<VkTrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            if (api == null || !api.IsAuthorized)
            {
                throw new InvalidOperationException("Not authorized");
            }

            if (TryAddAsCollection(query, tracks, is_playlist: true)) return tracks;
            if (TryAddAsCollection(query, tracks, is_playlist: false)) return tracks;

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
                ? api?.Audio.GetPlaylistById(user_l, id_l)
                : null;

            var vk_tracks = api?.Audio.Get(new AudioGetParams() { OwnerId = user_l, PlaylistId = id_l, });

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
