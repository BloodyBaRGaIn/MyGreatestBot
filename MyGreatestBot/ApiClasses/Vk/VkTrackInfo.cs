using MyGreatestBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using VkNet.Model;

namespace MyGreatestBot.ApiClasses.Vk
{
    /// <summary>
    /// Vk track info implementation
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class VkTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        public ITrackInfo Base => this;

        public string Domain => "https://www.vk.com/";

        public ApiIntents TrackType => ApiIntents.Vk;

        public string Id { get; }

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }

        public string Title => TrackName.Title;

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string? CoverURL { get; }
        public string AudioURL { get; private set; }
        public bool IsLiveStream => false;

        private readonly Audio origin;

        /// <summary>
        /// Vk track info constructor
        /// </summary>
        /// <param name="audio">Track instance from Vk API</param>
        /// <param name="playlist">Playlist instance from Vk API</param>
        internal VkTrackInfo(Audio audio, AudioPlaylist? playlist = null)
        {
            origin = audio;

            IEnumerable<AudioArtist> main_artists = audio.MainArtists;
            IEnumerable<AudioArtist> feat_artists = audio.FeaturedArtists;

            IEnumerable<AudioArtist> audioArtists;

            if (main_artists != null && main_artists.Any())
            {
                audioArtists = main_artists;
            }
            else if (feat_artists != null && feat_artists.Any())
            {
                audioArtists = feat_artists;
            }
            else
            {
                audioArtists = new[] { new AudioArtist() { Name = audio.Artist } };
            }

            ArtistArr = audioArtists.Select(a =>
                string.IsNullOrWhiteSpace(a.Id)
                ? new HyperLink(a.Name)
                : new HyperLink(a.Name, $"{Domain}artist/{a.Id}").WithId(a.Id))
                .ToArray();

            AudioAlbum album = audio.Album;

            TrackName = audio.OwnerId == null || audio.Id == null
                ? new(audio.Title)
                : new(audio.Title, $"{Domain}audio{audio.OwnerId}_{audio.Id}");

            if (playlist != null)
            {
                PlaylistName = playlist.OwnerId == null || playlist.Id == null
                    ? new(playlist.Title)
                    : new(playlist.Title, $"{Domain}music/playlist/{playlist.OwnerId}_{playlist.Id}");
            }

            if (album != null)
            {
                AlbumName = new(album.Title, $"{Domain}music/album/{album.OwnerId}_{album.Id}");

                CoverURL = album.Thumb?.Photo135;
            }
            else
            {
                AlbumName = null;
                CoverURL = null;
            }

            Id = audio.Id == null ? string.Empty : ((long)audio.Id).ToString();

            Duration = TimeSpan.FromSeconds(audio.Duration);

            AudioURL = string.Empty;
        }

        void ITrackInfo.ObtainAudioURL()
        {
            string? url = origin.Url?.ToString();
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Cannot get audio URL");
            }
            AudioURL = url;
        }

        void ITrackInfo.Reload()
        {
            ApiManager.ReloadApis(TrackType);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
