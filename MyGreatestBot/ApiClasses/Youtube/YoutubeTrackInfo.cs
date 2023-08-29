using MyGreatestBot.Utils;
using System;
using System.Runtime.Versioning;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace MyGreatestBot.ApiClasses.Youtube
{
    /// <summary>
    /// Youtube track info implementation
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal sealed class YoutubeTrackInfo : ITrackInfo, IComparable<ITrackInfo>
    {
        public ITrackInfo Base => this;

        public string Domain => "https://www.youtube.com/";

        public ApiIntents TrackType => ApiIntents.Youtube;

        public string Id { get; }

        public HyperLink TrackName { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName => null;
        public HyperLink? PlaylistName { get; }

        public string Title => TrackName.Title;

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string? CoverURL { get; }
        public string AudioURL { get; private set; }
        public bool IsLiveStream => Duration == TimeSpan.Zero;

        /// <summary>
        /// Youtube track info constructor
        /// </summary>
        /// <param name="video">Track instance from Youtube API</param>
        /// <param name="playlist">Playlist instance from Youtube API</param>
        internal YoutubeTrackInfo(IVideo video, Playlist? playlist = null)
        {
            Id = video.Id;

            TrackName = new(video.Title, video.Url);
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
                AudioURL = IsLiveStream
                    ? YoutubeApiWrapper.Streams
                        .GetHttpLiveStreamUrlAsync(Id)
                        .AsTask()
                        .GetAwaiter()
                        .GetResult()
                    : YoutubeApiWrapper.Streams
                        .GetManifestAsync(Id)
                        .AsTask()
                        .GetAwaiter()
                        .GetResult()
                        .GetAudioOnlyStreams()
                        .GetWithHighestBitrate().Url;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Cannot get audio URL", ex);
            }
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
