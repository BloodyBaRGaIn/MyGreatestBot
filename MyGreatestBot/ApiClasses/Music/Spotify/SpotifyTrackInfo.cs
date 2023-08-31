using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.Utils;
using SpotifyAPI.Web;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    /// <summary>
    /// Spotify track info implementation
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class SpotifyTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        private ITrackInfo Base => this;

        string ITrackInfo.Domain => "https://open.spotify.com/";

        ApiIntents ITrackInfo.TrackType => ApiIntents.Spotify;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }

        public TimeSpan Duration { get; private set; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; }
        public string? CoverURL { get; }

        /// <summary>
        /// Spotify track info constructor
        /// </summary>
        /// <param name="track">Track instance from Spotify API</param>
        /// <param name="playlist">Playlist instance from Spotify API</param>
        internal SpotifyTrackInfo(FullTrack track, FullPlaylist? playlist = null)
        {
            TrackName = new HyperLink(track.Name, $"{Base.Domain}track/{track.Id}").WithId(track.Id);

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(
                    a.Name,
                    $"{Base.Domain}artist/{a.Id}").WithId(a.Id)).ToArray();

            AlbumName = new(track.Album.Name, $"{Base.Domain}album/{track.Album.Id}");

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            PlaylistName = playlist == null || string.IsNullOrWhiteSpace(playlist.Name)
                ? null
                : string.IsNullOrWhiteSpace(playlist.Id)
                    ? new(playlist.Name)
                    : new(playlist.Name, $"{Base.Domain}playlist/{playlist.Id}");

            CoverURL = track.Album.Images.FirstOrDefault()?.Url;

            // default
            AudioURL = track.PreviewUrl;
        }

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                ITrackInfo? result = YandexApiWrapper.SearchTrack(this);
                if (result != null)
                {
                    AudioURL = result.AudioURL;
                    Duration = result.Duration;
                }
                else
                {
                    Duration = TimeSpan.FromSeconds(29);
                }
                if (string.IsNullOrWhiteSpace(AudioURL))
                {
                    throw new ArgumentNullException(nameof(AudioURL));
                }
            }
            catch (Exception ex)
            {
                throw new SpotifyApiException("Cannot get audio URL", ex);
            }

            //var player = SpotifyApiWrapper.Player;

            //var devices = player.GetAvailableDevices().GetAwaiter().GetResult();

            //if (devices.Devices.Any(d => d.IsActive))
            //{
            //    player.TransferPlayback(new PlayerTransferPlaybackRequest(new List<string>() { devices.Devices.Find(d => d.IsActive).Id })).GetAwaiter().GetResult();

            //    bool result = player.AddToQueue(new PlayerAddToQueueRequest(_track_uri)).GetAwaiter().GetResult();

            //    var playback = player.GetCurrentPlayback().GetAwaiter().GetResult();

            //    var current = player.GetCurrentlyPlaying(new PlayerCurrentlyPlayingRequest()).GetAwaiter().GetResult();

            //    AudioURL = current.Context.Uri;
            //}
        }

        void ITrackInfo.Reload()
        {
            ApiManager.ReloadApis(Base.TrackType | ApiIntents.Yandex);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
