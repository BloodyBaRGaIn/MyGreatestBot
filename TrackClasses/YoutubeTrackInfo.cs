using DicordNET.ApiClasses;
using DicordNET.Utils;
using YoutubeExplode.Playlists;
using YoutubeExplode.Videos;
using YoutubeExplode.Videos.Streams;

namespace DicordNET.TrackClasses
{
    internal sealed class YoutubeTrackInfo : ITrackInfo
    {
        public HyperLink TrackName { get; }
        public string Id { get; }
        public HyperLink[] ArtistArr { get; }
        public HyperLink? AlbumName { get; }
        public HyperLink? PlaylistName { get; }
        public TimeSpan Duration { get; }
        public TimeSpan Seek { get; set; }
        public string? CoverURL { get; }
        public string AudioURL { get; set; }
        public bool IsLiveStream { get; set; }

        public ITrackInfo Base => this;

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

            try
            {
                StreamManifest manifest = YoutubeApiWrapper.Streams.GetManifestAsync(Id)
                                                                   .GetAwaiter()
                                                                   .GetResult();

                IEnumerable<AudioOnlyStreamInfo> audioStreams = manifest.GetAudioOnlyStreams();

                if (!audioStreams.Any())
                {
                    throw new InvalidOperationException("Cannot get audio");
                }

                var audioStream = audioStreams.Where(a => a.Bitrate == audioStreams.Max(s => s.Bitrate)).First();

                AudioURL = audioStream.Url;

                IsLiveStream = false;
            }
            catch
            {
                var stream_url = YoutubeApiWrapper.Streams.GetHttpLiveStreamUrlAsync(Id)
                                                  .GetAwaiter()
                                                  .GetResult();
                AudioURL = stream_url;

                IsLiveStream = true;
            }
        }

        internal YoutubeTrackInfo(IVideo video, Playlist? playlist = null)
        {
            Id = video.Id;

            TrackName = new(video.Title, video.Url);

            ArtistArr = new HyperLink[1] { new(video.Author.ChannelTitle, video.Author.ChannelUrl) };

            Duration = video.Duration ?? TimeSpan.Zero;
            AudioURL = string.Empty;
            CoverURL = $"https://img.youtube.com/vi/{video.Id}/mqdefault.jpg";

            PlaylistName = playlist == null ? null : new(playlist.Title, playlist.Url);
        }

        void ITrackInfo.Reload()
        {
            YoutubeApiWrapper.Logout();
            YoutubeApiWrapper.PerformAuth();
        }
    }
}
