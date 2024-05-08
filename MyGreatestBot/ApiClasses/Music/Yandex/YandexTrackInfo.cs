using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using Yandex.Music.Api.Extensions.API;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex track info implementation
    /// </summary>
    public sealed class YandexTrackInfo : ITrackInfo
    {
#pragma warning disable CA1859
        private ITrackInfo Base => this;
#pragma warning restore CA1859

        ApiIntents ITrackInfo.TrackType => ApiIntents.Yandex;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull]
        public HyperLink AlbumName { get; }
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.TimePosition { get; set; }

        public string AudioURL { get; private set; } = string.Empty;
        [AllowNull]
        public string CoverURL { get; } = null;

        bool ITrackInfo.Radio { get; set; }
        bool ITrackInfo.BypassCheck { get; set; }

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

            TrackName = new HyperLink(track.Title, $"{Base.Domain}track/{track.Id}").WithId(Base.GetCompositeId(track.Id));

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(transletters ? a.Name.ToTransletters() : a.Name, $"{Base.Domain}artist/{a.Id}")
                .WithId(Base.GetCompositeId(a.Id))).ToArray();

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            if (track.Albums != null && track.Albums.Count != 0)
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

            List<string?> cover_uris = [track.CoverUri];

            if (track.Albums != null && track.Albums.Count != 0)
            {
                cover_uris.AddRange(track.Albums.Select(a => a.CoverUri));
            }

            if (track.Artists.Count != 0)
            {
                cover_uris.AddRange(track.Artists.Select(a => a.OgImage));
            }

            foreach (string? cover in cover_uris)
            {
                if (!string.IsNullOrWhiteSpace(cover))
                {
                    string temp = $"https://{cover.Replace("/%%", "/100x100")}";
                    if (IAccessible.IsUrlSuccess(temp, false))
                    {
                        CoverURL = temp;
                        break;
                    }
                }
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
    }
}
