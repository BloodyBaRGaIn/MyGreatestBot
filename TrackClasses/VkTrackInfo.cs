﻿using DicordNET.ApiClasses;
using DicordNET.Utils;
using VkNet.Model;

namespace DicordNET.TrackClasses
{
    internal sealed class VkTrackInfo : ITrackInfo
    {
        public ITrackInfo Base => this;

        public string Domain => "https://www.vk.com/";

        public string Id { get; }
        public string Title => Base.Title;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }
        
        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string? CoverURL { get; }
        public string AudioURL { get; private set; }
        public bool IsLiveStream => false;

        private readonly Audio origin;

        internal VkTrackInfo(Audio audio, AudioPlaylist? playlist = null)
        {
            origin = audio;

            var main_artists = audio.MainArtists;
            var feat_artists = audio.FeaturedArtists;

            List<HyperLink> list = new();

            if (main_artists != null && main_artists.Any())
            {
                foreach (var artist in main_artists)
                {
                    list.Add(string.IsNullOrWhiteSpace(artist.Id)
                        ? new(artist.Name)
                        : new(artist.Name, $"{Domain}artist/{artist.Id}"));
                }
            }
            else if (feat_artists != null && feat_artists.Any())
            {
                foreach (var artist in feat_artists)
                {
                    list.Add(string.IsNullOrWhiteSpace(artist.Id)
                        ? new(artist.Name)
                        : new(artist.Name, $"{Domain}artist/{artist.Id}"));
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
            string url = origin.Url.ToString();
            if (string.IsNullOrWhiteSpace(url))
            {
                throw new InvalidOperationException("Cannot get audio URL");
            }
            AudioURL = url;
        }

        void ITrackInfo.Reload()
        {
            ApiConfig.ReloadApis(ApiIntents.Vk);
        }
    }
}
