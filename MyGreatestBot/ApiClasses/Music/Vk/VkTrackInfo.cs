using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using VkNet.Model;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    /// <summary>
    /// Vk track info implementation
    /// </summary>
    public sealed class VkTrackInfo : ITrackInfo
    {
        private ITrackInfo Base => this;

        ApiIntents ITrackInfo.TrackType => ApiIntents.Vk;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull]
        public HyperLink AlbumName { get; }
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.TimePosition { get; set; }

        [AllowNull]
        public string CoverURL { get; } = null;
        public string AudioURL { get; private set; }

        bool ITrackInfo.Radio { get; set; }
        bool ITrackInfo.BypassCheck { get; set; }

        private readonly Audio origin;

        /// <summary>
        /// Vk track info constructor
        /// </summary>
        /// <param name="audio">Track instance from Vk API</param>
        /// <param name="playlist">Playlist instance from Vk API</param>
        internal VkTrackInfo(Audio audio, AudioPlaylist? playlist = null)
        {
            origin = audio;

            string id_str = audio.Id?.ToString() ?? string.Empty;

            TrackName = audio.OwnerId == null || string.IsNullOrEmpty(id_str)
                ? new(audio.Title)
                : new HyperLink(audio.Title, $"{Base.Domain}audio{audio.OwnerId}_{id_str}").WithId(id_str);

            IEnumerable<AudioArtist> audioArtists = Enumerable.Empty<AudioArtist>()
                .Concat(audio.MainArtists)
                .Concat(audio.FeaturedArtists)
                .DistinctBy(a => a.Name);

            if (!audioArtists.Any())
            {
                audioArtists = [new AudioArtist() { Name = audio.Artist }];
            }

            ArtistArr = audioArtists.Select(a =>
                string.IsNullOrWhiteSpace(a.Id)
                ? new HyperLink(a.Name)
                : new HyperLink(a.Name, $"{Base.Domain}artist/{a.Id}").WithId(a.Id))
                .ToArray();

            AudioAlbum album = audio.Album;

            AlbumName = album == null
                ? null
                : new(album.Title, $"{Base.Domain}music/album/{album.OwnerId}_{album.Id}");

            PlaylistName = playlist == null
                ? null
                : playlist.OwnerId == null || playlist.Id == null
                    ? new(playlist.Title)
                    : new(playlist.Title, $"{Base.Domain}music/playlist/{playlist.OwnerId}_{playlist.Id}");

            string? cover = album?.Thumb?.Photo135;
            if (!string.IsNullOrWhiteSpace(cover) && IAccessible.IsUrlSuccess(cover))
            {
                CoverURL = cover;
            }

            Duration = TimeSpan.FromSeconds(audio.Duration);

            AudioURL = string.Empty;
        }

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                AudioURL = origin.Url?.ToString() ?? string.Empty;
                if (string.IsNullOrWhiteSpace(AudioURL))
                {
                    throw new ArgumentNullException(nameof(AudioURL));
                }
            }
            catch (Exception? ex)
            {
                throw new VkApiException("Cannot get audio URL", ex);
            }
        }
    }
}
