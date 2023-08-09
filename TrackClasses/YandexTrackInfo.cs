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
        public ITrackInfo Base => this;

        public string Domain => "https://music.yandex.ru/";

        public string Id { get; }
        public string Title => Base.Title;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }
        
        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; }
        public string? CoverURL { get; }
        public bool IsLiveStream => false;

        internal YandexTrackInfo(YTrack track, YPlaylist? playlist = null)
        {
            Id = track.Id;

            TrackName = new(track.Title, $"{Domain}track/{Id}");

            List<YArtist> authors_list = track.Artists;

            ArtistArr = new HyperLink[authors_list.Count];

            for (int i = 0; i < authors_list.Count; i++)
            {
                YArtist artist = authors_list[i];
                ArtistArr[i] = new(artist.Name, $"{Domain}artist/{artist.Id}");
            }

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            AudioURL = string.Empty;

            if (!string.IsNullOrWhiteSpace(track.CoverUri))
            {
                CoverURL = $"https://{track.CoverUri.Replace("/%%", "/100x100")}";
            }

            if (track.Albums.Any())
            {
                YAlbum album = track.Albums.First();
                AlbumName = new(album.Title, $"{Domain}album/{album.Id}");
            }

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, $"{Domain}users/{playlist.Owner.Login}/playlists/{playlist.Kind}");
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
            int retries = 0;
            while (true)
            {
                if (retries > 2)
                {
                    throw new InvalidOperationException("Cannot get audio URL");
                }
                AudioURL = YandexApiWrapper.GetAudioURL(Id);
                if (string.IsNullOrEmpty(AudioURL))
                {
                    Base.Reload();
                    retries++;
                }
                else break;
            }
        }

        void ITrackInfo.Reload()
        {
            ApiConfig.DeinitApis(ApiIntents.Yandex);
        }
    }
}
