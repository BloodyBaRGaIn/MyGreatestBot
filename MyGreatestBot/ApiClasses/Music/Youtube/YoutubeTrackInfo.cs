using MyGreatestBot.ApiClasses.Utils;
using System;
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
        [AllowNull]
        public HyperLink AlbumName { get; } = null;
        [AllowNull]
        public HyperLink PlaylistName { get; }

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.TimePosition { get; set; }

        [AllowNull]
        public string CoverURL { get; }
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

        void ITrackInfo.ObtainAudioURL()
        {
            try
            {
                YoutubeApiWrapper _api_instance = YoutubeApiWrapper.MusicInstance as YoutubeApiWrapper ?? throw new ArgumentNullException(nameof(YoutubeApiWrapper));

                AudioURL = Base.IsLiveStream
                    ? _api_instance.Streams
                        .GetHttpLiveStreamUrlAsync(Base.Id)
                        .AsTask()
                        .GetAwaiter()
                        .GetResult()
                    : _api_instance.Streams
                        .GetManifestAsync(Base.Id)
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
    }
}
