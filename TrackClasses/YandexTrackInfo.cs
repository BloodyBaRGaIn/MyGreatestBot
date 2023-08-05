using DicordNET.ApiClasses;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Artist;
using Yandex.Music.Api.Models.Track;

namespace DicordNET.TrackClasses
{
    internal sealed class YandexTrackInfo : ITrackInfo
    {
        private const string DomainURL = "https://music.yandex.ru/";

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public string Id { get; }
        public TimeSpan Duration { get; }
        public TimeSpan Seek { get; set; }
        public string AudioURL { get; set; }
        public string CoverURL { get; }
        public bool IsLiveStream { get => false; set => throw new NotImplementedException(); }

        internal YandexTrackInfo(YTrack track)
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
        }

        void ITrackInfo.ObtainAudioURL()
        {
        start:
            AudioURL = YandexApiWrapper.GetAudioURL(Id);
            if (string.IsNullOrEmpty(AudioURL))
            {
                (this as ITrackInfo).Reload();
                goto start;
            }
        }

        void ITrackInfo.Reload()
        {
            YandexApiWrapper.Init();
        }
    }
}
