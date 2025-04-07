using MyGreatestBot.ApiClasses.Utils;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Threading;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    /// <summary>
    /// Spotify track info implementation
    /// </summary>
    public sealed class SpotifyTrackInfo : BaseTrackInfo
    {
        public override ApiIntents TrackType => ApiIntents.Spotify;

        private ApiIntents AudioFrom = ApiIntents.None;

        /// <summary>
        /// Spotify track info constructor
        /// </summary>
        /// <param name="track">Track instance from Spotify API</param>
        /// <param name="playlist">Playlist instance from Spotify API</param>
        internal SpotifyTrackInfo(FullTrack track, FullPlaylist? playlist = null)
        {
            TrackName = new HyperLink(track.Name, $"{Domain}track/{track.Id}")
                .WithId(GetCompositeId(track.Id));

            ArtistArr = [.. track.Artists.Select(a =>
                new HyperLink(
                    a.Name,
                    $"{Domain}artist/{a.Id}")
                .WithId(GetCompositeId(a.Id)))];

            AlbumName = new(track.Album.Name, $"{Domain}album/{track.Album.Id}");

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            PlaylistName = playlist == null || string.IsNullOrWhiteSpace(playlist.Name)
                ? null
                : string.IsNullOrWhiteSpace(playlist.Id)
                ? new(playlist.Name)
                : new(playlist.Name, $"{Domain}playlist/{playlist.Id}");

            CoverUrlCollection = track.Album.Images?.Select(i => i?.Url);

            // default
            AudioURL = track.PreviewUrl;
        }

        private bool TrySearchGeneric(ISearchMusicAPI instance, CancellationTokenSource cts)
        {
            try
            {
                BaseTrackInfo? result = instance.SearchTrack(this);
                ArgumentNullException.ThrowIfNull(result);

                result.ObtainAudioURL(Timeout.Infinite, cts);

                AudioFrom = instance.ApiType;
                AudioURL = result.AudioURL;
                Duration = result.Duration;
                return true;
            }
            catch
            {
                return false;
            }
        }

        protected override void ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            if (!TrySearchGeneric(YoutubeApiWrapper.SearchMusicInstance, cts))
            {
                // default Spotify preview track duration
                Duration = TimeSpan.FromSeconds(29);
            }
        }

        /// <inheritdoc cref="BaseTrackInfo.Reload"/>
        public new void Reload()
        {
            ApiManager.ReloadApis(TrackType | AudioFrom);
        }
    }
}
