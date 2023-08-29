using MyGreatestBot.Extensions;
using MyGreatestBot.Utils;
using System;
using System.Linq;
using System.Runtime.Versioning;
using Yandex.Music.Api.Models.Album;
using Yandex.Music.Api.Models.Playlist;
using Yandex.Music.Api.Models.Track;

namespace MyGreatestBot.ApiClasses.Music.Yandex
{
    /// <summary>
    /// Yandex track info implementation
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class YandexTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        public ITrackInfo Base => this;

        public string Domain => "https://music.yandex.ru/";

        public ApiIntents TrackType => ApiIntents.Yandex;

        public string Id { get; }

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }

        public string Title => TrackName.Title;

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string AudioURL { get; private set; }
        public string? CoverURL { get; }
        public bool IsLiveStream => false;

        /// <summary>
        /// Yandex track info constructor
        /// </summary>
        /// <param name="track">Track instance from Yandex API</param>
        /// <param name="playlist">Playlist instance from Yandex API</param>
        /// <param name="transletters">Make transletters from cyrillic</param>
        internal YandexTrackInfo(YTrack track, YPlaylist? playlist = null, bool transletters = false)
        {
            Id = track.Id;

            TrackName = new(track.Title, $"{Domain}track/{Id}");

            ArtistArr = track.Artists.Select(a =>
                new HyperLink(transletters ? a.Name.ToTransletters() : a.Name, $"{Domain}artist/{a.Id}").WithId(a.Id)).ToArray();

            Duration = TimeSpan.FromMilliseconds(track.DurationMs);

            AudioURL = string.Empty;

            if (!string.IsNullOrWhiteSpace(track.CoverUri))
            {
                CoverURL = $"https://{track.CoverUri.Replace("/%%", "/100x100")}";
            }

            if (track.Albums != null && track.Albums.Any())
            {
                YAlbum album = track.Albums.First();
                AlbumName = new(album.Title, $"{Domain}album/{album.Id}");
            }

            if (playlist != null)
            {
                string title = playlist.Title;
                if (title == string.Empty)
                {
                    title = "Playlist";
                }
                PlaylistName = new(title, $"{Domain}users/{playlist.Owner.Login}/playlists/{playlist.Kind}");
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
            int retries = 0;
            while (true)
            {
                if (retries > 2)
                {
                    throw new YandexApiException("Cannot get audio URL");
                }
                AudioURL = YandexApiWrapper.GetAudioURL(Id);
                if (string.IsNullOrEmpty(AudioURL))
                {
                    Base.Reload();
                    retries++;
                }
                else
                {
                    break;
                }
            }
        }

        void ITrackInfo.Reload()
        {
            ApiManager.DeinitApis(TrackType);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
