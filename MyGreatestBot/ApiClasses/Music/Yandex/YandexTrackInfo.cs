using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;
using Yandex.Music.Client.Extensions;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex track info implementation
    /// </summary>
    public sealed class YandexTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        private ITrackInfo Base => this;

        ApiIntents ITrackInfo.TrackType => ApiIntents.Yandex;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull]
        public HyperLink AlbumName { get; }
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; } = string.Empty;
        [AllowNull]
        public string CoverURL { get; }

        private readonly YTrack origin;

        /// <summary>
        /// Yandex track info constructor
        /// </summary>
        /// <param name="track">Track instance from Yandex API</param>
        /// <param name="playlist">Playlist instance from Yandex API</param>
        /// <param name="transletters">Make transletters from cyrillic</param>
        internal YandexTrackInfo(YTrack track, YPlaylist? playlist = null, bool transletters = false)
        {
            origin = track;

            TrackName = new HyperLink(track.Title, $"{Base.Domain}track/{track.Id}").WithId(track.Id);

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(transletters ? a.Name.ToTransletters() : a.Name, $"{Base.Domain}artist/{a.Id}").WithId(a.Id)).ToArray();

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            if (track.Albums != null && track.Albums.Any())
            {
                YAlbum album = track.Albums.First();
                AlbumName = new(album.Title, $"{Base.Domain}album/{album.Id}");
            }

            if (playlist != null)
            {
                string title = playlist.Title;
                if (title == string.Empty)
                {
                    title = "Playlist";
                }
                PlaylistName = new(title, $"{Base.Domain}users/{playlist.Owner.Login}/playlists/{playlist.Kind}");
            }

            if (!string.IsNullOrWhiteSpace(track.CoverUri))
            {
                CoverURL = $"https://{track.CoverUri.Replace("/%%", "/100x100")}";
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                AudioURL = origin.GetLink();
                if (string.IsNullOrWhiteSpace(AudioURL))
                {
                    throw new ArgumentNullException(nameof(AudioURL));
                }
            }
            catch (Exception ex)
            {
                throw new YandexApiException("Cannot get audio URL", ex);
            }
        }

        public int CompareTo([AllowNull] ITrackInfo other)
        {
            return Base.CompareTo(other);
        }
    }
}
