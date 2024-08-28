using MyGreatestBot.ApiClasses.Utils;
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
    public sealed class YoutubeTrackInfo : BaseTrackInfo
    {
        public override string Domain => "https://www.youtube.com/";

        public override ApiIntents TrackType => ApiIntents.Youtube;

        /// <summary>
        /// Youtube track info constructor
        /// </summary>
        /// <param name="video">Track instance from Youtube API</param>
        /// <param name="playlist">Playlist instance from Youtube API</param>
        internal YoutubeTrackInfo(IVideo video, Playlist? playlist = null)
        {
            TrackName = new HyperLink(video.Title, video.Url)
                .WithId(GetCompositeId(video.Id));

            ArtistArr =
            [
                new HyperLink(video.Author.ChannelTitle, video.Author.ChannelUrl)
                    .WithId(GetCompositeId(video.Author.ChannelId))
            ];

            Duration = video.Duration ?? TimeSpan.Zero;

            AudioURL = string.Empty;

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, playlist.Url);
            }

            CoverUrlCollection = [$"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg"];
        }

        protected override void ObtainAudioURLInternal(CancellationTokenSource cts)
        {
            AudioURL = (IsLiveStream
                ? YoutubeApiWrapper.Instance.Streams.GetHttpLiveStreamUrlAsync(Id, cts.Token)
                .AsTask().GetAwaiter().GetResult()
                : YoutubeApiWrapper.Instance.Streams.GetManifestAsync(Id, cts.Token)
                .AsTask().GetAwaiter().GetResult().GetAudioOnlyStreams()
                .TryGetWithHighestBitrate()?.Url) ?? string.Empty;
        }
    }
}
