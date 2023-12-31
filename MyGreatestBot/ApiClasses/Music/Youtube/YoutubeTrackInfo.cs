﻿using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    /// <summary>
    /// Youtube track info implementation
    /// </summary>
    public sealed class YoutubeTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        private ITrackInfo Base => this;

        string ITrackInfo.Domain => "https://www.youtube.com/";

        ApiIntents ITrackInfo.TrackType => ApiIntents.Youtube;

        public string Id { get; }

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull]
        public HyperLink AlbumName { get; } = null;
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        [AllowNull]
        public string CoverURL { get; }
        public string AudioURL { get; private set; }

        bool ITrackInfo.BypassCheck { get; set; } = false;

        /// <summary>
        /// Youtube track info constructor
        /// </summary>
        /// <param name="video">Track instance from Youtube API</param>
        /// <param name="playlist">Playlist instance from Youtube API</param>
        internal YoutubeTrackInfo(IVideo video, Playlist? playlist = null)
        {
            Id = video.Id;

            TrackName = new HyperLink(video.Title, video.Url).WithId(video.Id);
            ArtistArr = new HyperLink[1]
            {
                new HyperLink(video.Author.ChannelTitle, video.Author.ChannelUrl)
                    .WithId(video.Author.ChannelId.Value)
            };

            Duration = video.Duration ?? TimeSpan.Zero;

            AudioURL = string.Empty;
            CoverURL = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, playlist.Url);
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                YoutubeApiWrapper _api_instance = YoutubeApiWrapper.Instance as YoutubeApiWrapper ?? throw new ArgumentNullException(nameof(YoutubeApiWrapper));

                AudioURL = Base.IsLiveStream
                    ? _api_instance.Streams
                        .GetHttpLiveStreamUrlAsync(Id)
                        .AsTask()
                        .GetAwaiter()
                        .GetResult()
                    : _api_instance.Streams
                        .GetManifestAsync(Id)
                        .AsTask()
                        .GetAwaiter()
                        .GetResult()
                        .GetAudioOnlyStreams()
                        .GetWithHighestBitrate().Url;

                if (string.IsNullOrWhiteSpace(AudioURL))
                {
                    throw new ArgumentNullException(nameof(AudioURL));
                }
            }
            catch (Exception ex)
            {
                throw new YoutubeApiException("Cannot get audio URL", ex);
            }
        }

        public int CompareTo([AllowNull] ITrackInfo other)
        {
            return Base.CompareTo(other);
        }
    }
}
