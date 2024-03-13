using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
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
#pragma warning disable CA1859
        private ITrackInfo Base => this;
#pragma warning restore CA1859

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
        public string CoverURL { get; } = null;

        bool ITrackInfo.Radio { get; set; }
        bool ITrackInfo.BypassCheck { get; set; }

        private ApiIntents AudioFrom = ApiIntents.None;

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

            string? cover = track.Album.Images.FirstOrDefault()?.Url;

            if (!string.IsNullOrWhiteSpace(cover) && IAccessible.IsUrlSuccess(cover, false))
            {
                CoverURL = cover;
            }

            // default
            AudioURL = track.PreviewUrl;
        }

        private bool TrySearchGeneric(IMusicAPI instance)
        {
            try
            {
                ISearchable searchable = instance as ISearchable ?? throw new ApiException(instance.ApiType);
                ITrackInfo? result = searchable?.SearchTrack(this);
                if (result == null)
                {
                    return false;
                }

                result.ObtainAudioURL();

                AudioFrom = instance.ApiType;
                AudioURL = result.AudioURL;
                //Duration = result.Duration;
                return true;
            }
            catch
            {
                return false;
            }
        }

        private bool TrySearchYandex()
        {
            return TrySearchGeneric(YandexApiWrapper.Instance);
        }

        private bool TrySearchYoutube()
        {
            return TrySearchGeneric(YoutubeApiWrapper.Instance);
        }

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                switch (AudioFrom)
                {
                    case ApiIntents.Youtube:
                        _ = TrySearchYoutube();
                        return;

                    case ApiIntents.Yandex:
                        _ = TrySearchYandex();
                        return;

                    default:
                        if (TrySearchYoutube())
                        {
                            return;
                        }
                        if (TrySearchYandex())
                        {
                            return;
                        }

                        break;
                }

                // default Spotify preview track duration
                Duration = TimeSpan.FromSeconds(29);

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
            ApiManager.ReloadApis(Base.TrackType | AudioFrom);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
