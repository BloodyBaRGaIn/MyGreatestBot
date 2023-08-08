using DicordNET.ApiClasses;
using DicordNET.Utils;
using VkNet.Model;

namespace DicordNET.TrackClasses
{
    internal sealed class VkTrackInfo : ITrackInfo
    {
        private const string DomainUrl = "https://vk.com/";

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }
        public string Id { get; }
        public TimeSpan Duration { get; }
        public TimeSpan Seek { get; set; }
        public string? CoverURL { get; }
        public string AudioURL { get; set; }
        public bool IsLiveStream { get => false; set => throw new NotImplementedException(); }

        public ITrackInfo Base => this;

        internal VkTrackInfo(Audio audio, AudioPlaylist? playlist = null)
        {
            var main_artists = audio.MainArtists;
            var feat_artists = audio.FeaturedArtists;

            List<HyperLink> list = new();

            if (main_artists != null && main_artists.Any())
            {
                foreach (var artist in main_artists)
                {
                    list.Add(string.IsNullOrWhiteSpace(artist.Id)
                        ? new(artist.Name)
                        : new(artist.Name, $"{DomainUrl}artist/{artist.Id}"));
                }
            }
            else if (feat_artists != null && feat_artists.Any())
            {
                foreach (var artist in feat_artists)
                {
                    list.Add(string.IsNullOrWhiteSpace(artist.Id)
                        ? new(artist.Name)
                        : new(artist.Name, $"{DomainUrl}artist/{artist.Id}"));
                }
            }
            else
            {
                list.Add(new(audio.Artist));
            }

            ArtistArr = list.ToArray();

            AudioAlbum album = audio.Album;

            TrackName = audio.OwnerId == null || audio.Id == null
                ? new(audio.Title)
                : new(audio.Title, $"{DomainUrl}audio{audio.OwnerId}_{audio.Id}");

            if (playlist != null)
            {
                PlaylistName = playlist.OwnerId == null || playlist.Id == null
                    ? new(playlist.Title)
                    : new(playlist.Title, $"{DomainUrl}music/playlist/{playlist.OwnerId}_{playlist.Id}");
            }

            if (album != null)
            {
                AlbumName = new(album.Title, $"{DomainUrl}music/album/{album.OwnerId}_{album.Id}");

                CoverURL = album.Thumb?.Photo135;
            }
            else
            {
                AlbumName = null;
                CoverURL = null;
            }

            Id = audio.Id == null ? string.Empty : ((long)audio.Id).ToString();

            Duration = audio.Duration > 0 ? TimeSpan.FromSeconds(audio.Duration) : TimeSpan.Zero;

            AudioURL = audio.Url?.ToString() ?? string.Empty;
        }

        void ITrackInfo.ObtainAudioURL()
        {

        }

        void ITrackInfo.Reload()
        {
            VkApiWrapper.PerformAuth();
        }
    }
}
