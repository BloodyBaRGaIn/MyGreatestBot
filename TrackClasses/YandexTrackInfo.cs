using DicordNET.ApiClasses;
using DicordNET.Utils;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace DicordNET.TrackClasses
{
    internal sealed class YandexTrackInfo : ITrackInfo
    {
        private const string DomainURL = "https://music.yandex.ru/";

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

        internal YandexTrackInfo(YTrack track, YPlaylist? playlist = null)
        {
            Id = track.Id;

            TrackName = new(track.Title, $"{DomainURL}track/{Id}");

            List<YArtist> authors_list = track.Artists;

            ArtistArr = new HyperLink[authors_list.Count];

            for (int i = 0; i < authors_list.Count; i++)
            {
                YArtist artist = authors_list[i];
                ArtistArr[i] = new(artist.Name, $"{DomainURL}artist/{artist.Id}");
            }

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            AudioURL = string.Empty;

            if (string.IsNullOrWhiteSpace(track.CoverUri))
            {
                CoverURL = string.Empty;
            }
            else
            {
                CoverURL = $"https://{track.CoverUri.Replace("/%%", "/100x100")}";
            }

            if (track.Albums.Any())
            {
                YAlbum album = track.Albums.First();
                AlbumName = new(album.Title, $"{DomainURL}album/{album.Id}");
            }
            else
            {
                AlbumName = null;
            }

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, $"{DomainURL}users/{playlist.Owner.Login}/playlists/{playlist.Kind}");
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
        start:
            AudioURL = YandexApiWrapper.GetAudioURL(Id);
            if (string.IsNullOrEmpty(AudioURL))
            {
                Base.Reload();
                goto start;
            }
        }

        void ITrackInfo.Reload()
        {
            YandexApiWrapper.Logout();
            YandexApiWrapper.PerformAuth();
        }
    }
}
