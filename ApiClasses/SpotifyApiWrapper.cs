using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DicordNET.Config;
using DicordNET.TrackClasses;
using SpotifyAPI.Web;

namespace DicordNET.ApiClasses
{
    internal static class SpotifyApiWrapper
    {
        private static SpotifyClient? SpotifyClientInstance;

        private static class SpotifyQueryDecomposer
        {
            private static readonly Regex PLAYLIST_RE = new("");
            private static readonly Regex ALBUM_RE = new("");
            private static readonly Regex ARTIST_RE = new("");
            private static readonly Regex TRACK_RE = new("");
        }

        internal static void PerformAuth()
        {
            SpotifyClientSecretsJSON spotifyClientSecrets = ConfigManager.GetSpotifyClientSecretsJSON();

            SpotifyClientConfig config = SpotifyClientConfig.CreateDefault()
                .WithAuthenticator(
                    new ClientCredentialsAuthenticator(
                        spotifyClientSecrets.ClientId,
                        spotifyClientSecrets.ClientSecret
                        )
                    );

            if (SpotifyClientInstance != null)
            {
                lock (SpotifyClientInstance)
                {
                    SpotifyClientInstance = new(config);
                }
            }
            else
            {
                SpotifyClientInstance = new(config);
            }
        }

        internal static void Logout()
        {

        }

        internal static List<ITrackInfo> GetTracks(string? query)
        {
            List<ITrackInfo> tracks = new();

            if (string.IsNullOrWhiteSpace(query))
            {
                return tracks;
            }

            return tracks;
        }
    }
}
