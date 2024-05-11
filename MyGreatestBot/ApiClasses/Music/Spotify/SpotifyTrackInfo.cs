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
    public sealed class SpotifyTrackInfo : ITrackInfo
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
        TimeSpan ITrackInfo.TimePosition { get; set; }

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
            TrackName = new HyperLink(track.Name, $"{Base.Domain}track/{track.Id}")
                .WithId(Base.GetCompositeId(track.Id));

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(
                    a.Name,
                    $"{Base.Domain}artist/{a.Id}").WithId(Base.GetCompositeId(a.Id))).ToArray();

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

        private bool TrySearchGeneric(ISearchMusicAPI instance)
        {
            try
            {
                ITrackInfo? result = instance.SearchTrack(this);
                if (result == null)
                {
                    return false;
                }

                result.ObtainAudioURL();

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

        void ITrackInfo.ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            if (!TrySearchGeneric(YoutubeApiWrapper.SearchMusicInstance))
            {
                // default Spotify preview track duration
                Duration = TimeSpan.FromSeconds(29);
            }
        }

        void ITrackInfo.Reload()
        {
            ApiManager.ReloadApis(Base.TrackType | AudioFrom);
        }
    }
}
