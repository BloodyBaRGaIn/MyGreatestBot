using DicordNET.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DicordNET.ApiClasses.Youtube
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
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }

        public string Title => TrackName.Title;

        public TimeSpan Duration { get; }
        TimeSpan ITrackInfo.Seek { get; set; }

        public string? CoverURL { get; }
        public string AudioURL { get; private set; }
        public bool IsLiveStream { get; private set; }

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

            IsLiveStream = Duration == TimeSpan.Zero;

            AudioURL = string.Empty;
            CoverURL = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";

            if (playlist != null)
            {
                PlaylistName = new(playlist.Title, playlist.Url);
            }
        }

        void ITrackInfo.ObtainAudioURL()
        {
            if (string.IsNullOrWhiteSpace(Id))
            {
                throw new InvalidOperationException("No results");
            }

            if (YoutubeApiWrapper.YoutubeClientInstance == null)
            {
                throw new ArgumentNullException(nameof(YoutubeApiWrapper.YoutubeClientInstance));
            }

            if (IsLiveStream)
            {
                string stream_url = YoutubeApiWrapper.Streams.GetHttpLiveStreamUrlAsync(Id)
                    .AsTask()
                    .GetAwaiter()
                    .GetResult() ?? throw new InvalidOperationException("Stream URL was null");

                AudioURL = stream_url;
            }
            else
            {
                StreamManifest manifest = YoutubeApiWrapper.Streams.GetManifestAsync(Id)
                    .AsTask()
                    .GetAwaiter()
                    .GetResult() ?? throw new InvalidOperationException("Manifest was null");

                IEnumerable<AudioOnlyStreamInfo> audioStreams = manifest.GetAudioOnlyStreams();

                if (!audioStreams.Any())
                {
                    throw new InvalidOperationException("No streams found");
                }

                long bps = audioStreams.Max(s => s.Bitrate.BitsPerSecond);

                AudioOnlyStreamInfo audioStream = audioStreams
                    .Where(a => a.Bitrate.BitsPerSecond == bps)
                    .First() ?? throw new InvalidOperationException("Stream URL was null");

                AudioURL = audioStream.Url;
            }
        }

        void ITrackInfo.Reload()
        {
            ApiConfig.ReloadApis(TrackType);
        }

        public int CompareTo(ITrackInfo? other)
        {
            return Base.CompareTo(other);
        }
    }
}
