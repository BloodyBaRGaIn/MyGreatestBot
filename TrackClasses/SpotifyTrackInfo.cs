using DicordNET.ApiClasses;
using DicordNET.Utils;
using SpotifyAPI.Web;
using libspotifydotnet;

namespace DicordNET.TrackClasses
{
    internal sealed class SpotifyTrackInfo : ITrackInfo
    {
        private const string DomainURL = "https://open.spotify.com/";

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }
        public string Id { get; }
        public TimeSpan Duration { get; }
        public TimeSpan Seek { get; set; }
        public string AudioURL { get; set; }
        public string? CoverURL { get; }
        public bool IsLiveStream { get => false; set => throw new NotImplementedException(); }

        public ITrackInfo Base => this;

        private readonly string _track_uri;

        internal SpotifyTrackInfo(IPlayableItem item, FullPlaylist? playlist = null)
        {
            switch (item)
            {
                case FullTrack track:
                    Id = track.Id;
                    TrackName = new(track.Name, $"{DomainURL}track/{Id}");
                    ArtistArr = track.Artists.Select(a => new HyperLink(a.Name, $"{DomainURL}artist/{a.Id}")).ToArray();
                    AlbumName = new(track.Album.Name, $"{DomainURL}album/{track.Album.Id}");

                    Duration = TimeSpan.FromMilliseconds(track.DurationMs);

                    if (playlist == null)
                    {
                        PlaylistName = null;
                    }
                    else if (string.IsNullOrWhiteSpace(playlist.Name))
                    {
                        PlaylistName = null;
                    }
                    else if (string.IsNullOrWhiteSpace(playlist.Id))
                    {
                        PlaylistName = new(playlist.Name);
                    }
                    else
                    {
                        PlaylistName = new(playlist.Name, $"{DomainURL}playlist/{playlist.Id}");
                    }

                    CoverURL = track.Album.Images.FirstOrDefault()?.Url;

                    AudioURL = track.PreviewUrl;

                    _track_uri = track.Uri;

                    break;

                default:
                    throw new ArgumentException(nameof(item));
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
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
            ApiConfig.ReloadApis(ApiIntents.Spotify);
        }
    }
}
