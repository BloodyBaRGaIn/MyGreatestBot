using DicordNET.ApiClasses;
using DicordNET.Utils;
using SpotifyAPI.Web;

namespace DicordNET.TrackClasses
{
    internal sealed class SpotifyTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        public ITrackInfo Base => this;

        public string Domain => "https://open.spotify.com/";

        public ApiIntents TrackType => ApiIntents.Spotify;

        public string Id { get; }

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }

        public string Title => TrackName.Title;

        public TimeSpan Duration { get; private set; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; }
        public string? CoverURL { get; }
        public bool IsLiveStream => false;

        internal SpotifyTrackInfo(FullTrack track, FullPlaylist? playlist = null)
        {
            Id = track.Id;
            TrackName = new(track.Name, $"{Domain}track/{Id}");

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(
                    //SpotifyApiWrapper.Artists.Get(a.Id).GetAwaiter().GetResult().Name,
                    a.Name,
                    $"{Domain}artist/{a.Id}")).ToArray();

            AlbumName = new(track.Album.Name, $"{Domain}album/{track.Album.Id}");

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            PlaylistName = playlist == null || string.IsNullOrWhiteSpace(playlist.Name)
                ? null
                : string.IsNullOrWhiteSpace(playlist.Id)
                    ? new(playlist.Name)
                    : new(playlist.Name, $"{Domain}playlist/{playlist.Id}");

            CoverURL = track.Album.Images.FirstOrDefault()?.Url;

            // default
            AudioURL = track.PreviewUrl;
        }

        void ITrackInfo.ObtainAudioURL()
        {
            var result = YandexApiWrapper.Search(this);
            if (result != null)
            {
                AudioURL = result.AudioURL;
                Duration = result.Duration;
            }
            else
            {
                Duration = TimeSpan.FromSeconds(30);
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
            ApiConfig.ReloadApis(TrackType | ApiIntents.Yandex);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
