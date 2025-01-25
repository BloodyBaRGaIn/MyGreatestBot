using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using VkNet.Model;

namespace MyGreatestBot.ApiClasses.Music.Vk
{
    /// <summary>
    /// Vk track info implementation
    /// </summary>
    public sealed class VkTrackInfo : BaseTrackInfo
    {
        public override ApiIntents TrackType => ApiIntents.Vk;

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
                : new HyperLink(audio.Title, $"{Domain}audio{audio.OwnerId}_{id_str}")
                    .WithId(GetCompositeId(id_str));

            IEnumerable<AudioArtist> audioArtists = Enumerable.Empty<AudioArtist>()
                .Concat(audio.MainArtists)
                .Concat(audio.FeaturedArtists)
                .DistinctBy(a => a.Name);

            if (!audioArtists.Any())
            {
                audioArtists = [new AudioArtist() { Name = audio.Artist }];
            }

            ArtistArr = [.. audioArtists.Select(a =>
                string.IsNullOrWhiteSpace(a.Id)
                ? new HyperLink(a.Name)
                : new HyperLink(a.Name, $"{Domain}artist/{a.Id}")
                    .WithId(GetCompositeId(a.Id)))];

            AudioAlbum album = audio.Album;

            AlbumName = album == null
                ? null
                : new(album.Title, $"{Domain}music/album/{album.OwnerId}_{album.Id}");

            PlaylistName = playlist == null
                ? null
                : playlist.OwnerId == null || playlist.Id == null
                    ? new(playlist.Title)
                    : new(playlist.Title, $"{Domain}music/playlist/{playlist.OwnerId}_{playlist.Id}");

            CoverUrlCollection = [album?.Thumb?.Photo135];

            Duration = TimeSpan.FromSeconds(audio.Duration);

            AudioURL = string.Empty;
        }

        protected override void ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            _ = cts;
            AudioURL = origin.Url?.ToString() ?? string.Empty;
        }
    }
}
