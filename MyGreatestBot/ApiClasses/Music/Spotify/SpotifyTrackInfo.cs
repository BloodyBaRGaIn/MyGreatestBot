using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Utils;
using SpotifyAPI.Web;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace MyGreatestBot.ApiClasses.Music.Spotify
{
    /// <summary>
    /// Spotify track info implementation
    /// </summary>
    public sealed class SpotifyTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        private ITrackInfo Base => this;

        ApiIntents ITrackInfo.TrackType => ApiIntents.Spotify;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull]
        public HyperLink AlbumName { get; }
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; private set; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; }
        [AllowNull]
        public string CoverURL { get; }

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
                YandexApiWrapper _api_instance = YandexApiWrapper.Instance as YandexApiWrapper ?? throw new ArgumentNullException(nameof(YandexApiWrapper));

                ITrackInfo? result = _api_instance.SearchTrack(this);
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

        public int CompareTo([AllowNull] ITrackInfo other)
        {
            return Base.CompareTo(other);
        }
    }
}
