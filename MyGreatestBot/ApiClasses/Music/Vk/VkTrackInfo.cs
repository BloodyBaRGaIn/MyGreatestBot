using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;
using VkNet.Model;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    /// <summary>
    /// Vk track info implementation
    /// </summary>
    [SupportedOSPlatform("windows")]
    public sealed class VkTrackInfo : ITrackInfo, IComparable<ITrackInfo>
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
        TimeSpan ITrackInfo.Seek { get; set; }

        [AllowNull]
        public string CoverURL { get; }
        public string AudioURL { get; private set; }

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

            CoverURL = album?.Thumb?.Photo135;

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

        public int CompareTo([AllowNull] ITrackInfo other)
        {
            return Base.CompareTo(other);
        }
    }
}
