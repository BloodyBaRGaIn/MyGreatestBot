﻿using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Threading;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MyGreatestBot.ApiClasses.Music.Youtube
{
    /// <summary>
    /// Youtube track info implementation
    /// </summary>
    public sealed class YoutubeTrackInfo : ITrackInfo
    {
#pragma warning disable CA1859
        private ITrackInfo Base => this;
#pragma warning restore CA1859

        string ITrackInfo.Domain => "https://www.youtube.com/";

        ApiIntents ITrackInfo.TrackType => ApiIntents.Youtube;

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        [AllowNull] public HyperLink AlbumName { get; } = null;
        [AllowNull] public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.TimePosition { get; set; }

        [AllowNull] public string CoverURL { get; }
        public string AudioURL { get; private set; }

        bool ITrackInfo.Radio { get; set; }
        bool ITrackInfo.BypassCheck { get; set; }

        /// <summary>
        /// Youtube track info constructor
        /// </summary>
        /// <param name="video">Track instance from Youtube API</param>
        /// <param name="playlist">Playlist instance from Youtube API</param>
        internal YoutubeTrackInfo(IVideo video, Playlist? playlist = null)
        {
            TrackName = new HyperLink(video.Title, video.Url)
                .WithId(Base.GetCompositeId(video.Id));

            ArtistArr =
            [
                new HyperLink(video.Author.ChannelTitle, video.Author.ChannelUrl)
                    .WithId(Base.GetCompositeId(video.Author.ChannelId))
            ];

            Duration = video.Duration ?? TimeSpan.Zero;

            AudioURL = string.Empty;

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, playlist.Url);
            }

            string cover = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";

            if (IAccessible.IsUrlSuccess(cover, false))
            {
                CoverURL = cover;
            }
        }

        void ITrackInfo.ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            AudioURL = (Base.IsLiveStream
                ? YoutubeApiWrapper.Instance.Streams.GetHttpLiveStreamUrlAsync(Base.Id, cts.Token)
                .AsTask().GetAwaiter().GetResult()
                : YoutubeApiWrapper.Instance.Streams.GetManifestAsync(Base.Id, cts.Token)
                .AsTask().GetAwaiter().GetResult().GetAudioOnlyStreams()
                .TryGetWithHighestBitrate()?.Url) ?? string.Empty;
        }
    }
}
