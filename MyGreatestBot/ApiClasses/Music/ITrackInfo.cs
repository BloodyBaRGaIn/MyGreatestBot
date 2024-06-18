using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Track information abstraction
    /// </summary>
    public interface ITrackInfo
    {
        /// <summary>
        /// Track type
        /// </summary>
        public virtual ApiIntents TrackType => ApiIntents.None;

        /// <summary>
        /// Base URL
        /// </summary>
        [DisallowNull]
        public virtual string Domain =>
            ApiManager.Get<IMusicAPI>(TrackType)?.Domains?.ToString() ?? string.Empty;

        /// <summary>
        /// Extended track name
        /// </summary>
        public HyperLink TrackName { get; }

        /// <summary>
        /// Extended artists collection
        /// </summary>
        public HyperLink[] ArtistArr { get; }

        /// <summary>
        /// Extended album name
        /// </summary>
        [AllowNull] public HyperLink AlbumName { get; }

        /// <summary>
        /// Extended playlist name
        /// </summary>
        [AllowNull] public HyperLink PlaylistName { get; }

        /// <summary>
        /// Base track name
        /// </summary>
        public string Title => TrackName.Title;

        /// <summary>
        /// Track ID
        /// </summary>
        public string Id => TrackName.InnerId;

        /// <summary>
        /// Current track duration
        /// </summary>
        public TimeSpan Duration { get; }

        /// <summary>
        /// Current track time position
        /// </summary>
        public TimeSpan TimePosition { get; protected set; }

        /// <summary>
        /// Thumbnails image URL
        /// </summary>
        [AllowNull] public string CoverURL { get; }

        /// <summary>
        /// Audio URL for FFMPEG
        /// </summary>
        public string AudioURL { get; }

        /// <summary>
        /// Indicates when a new track should be added, if possible.
        /// </summary>
        public bool Radio { get; set; }

        /// <summary>
        /// Indicates when a blacklist check should not be performed.
        /// </summary>
        public bool BypassCheck { get; set; }

        /// <summary>
        /// Does the track have a duration
        /// </summary>
        public bool IsLiveStream => Duration == TimeSpan.Zero;

        /// <summary>
        /// Check for seek operation possible
        /// </summary>
        /// <param name="span">Specified position</param>
        /// <returns>True if possible to seek</returns>
        public bool IsSeekPossible(TimeSpan span)
        {
            return !(IsLiveStream || span > Duration);
        }

        /// <summary>
        /// Performs seek operation with check
        /// </summary>
        /// <param name="span">Specified position</param>
        public void PerformSeek(TimeSpan span)
        {
            if (IsSeekPossible(span))
            {
                TimePosition = span;
            }
        }

        /// <summary>
        /// Get Discord message content
        /// </summary>
        /// <param name="state">For example "Playing"</param>
        /// <returns>Content string</returns>
        public string GetMessage(string state)
        {
            string result = string.Empty;
            result += $"{state}: {TrackName}{Environment.NewLine}";
            result += $"Author: {string.Join(", ", ArtistArr.Select(a => a.ToString()))}";

            if (!IsLiveStream)
            {
                result += $"{Environment.NewLine}Duration: {GetCustomTime(Duration)}";
            }

            if (AlbumName != null && !string.IsNullOrWhiteSpace(AlbumName.Title))
            {
                result += $"{Environment.NewLine}Album: {AlbumName}";
            }

            if (PlaylistName != null && !string.IsNullOrWhiteSpace(PlaylistName.Title))
            {
                result += $"{Environment.NewLine}Playlist: {PlaylistName}";
            }

            if (TimePosition != TimeSpan.Zero)
            {
                result += $"{Environment.NewLine}Time: {GetCustomTime(TimePosition)}";
            }

            return result;
        }

        public static string GetCustomTime(TimeSpan time, bool withMilliseconds = false)
        {
            static string GetPaddedValue(double x, int pad = 2)
            {
                return $"{(int)x}".PadLeft(pad, '0');
            }

            return $"{GetPaddedValue(time.TotalHours)}:{GetPaddedValue(time.Minutes)}:{GetPaddedValue(time.Seconds)}" +
                (withMilliseconds ? $":{GetPaddedValue(time.Milliseconds, 3)}" : string.Empty);
        }

        public string GetShortMessage(string prefix)
        {
            return $"{prefix}: {Title} by {string.Join(", ", ArtistArr.Select(a => a.Title))}";
        }

        /// <summary>
        /// Get Discord message thumbnail with track cover image
        /// </summary>
        /// <returns>Track cover image as thumbnail</returns>
        public DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail? GetThumbnail()
        {
            return string.IsNullOrWhiteSpace(CoverURL)
                ? null
                : new()
                {
                    Url = CoverURL
                };
        }

        /// <summary>
        /// Retrieves the audio URL
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ObtainAudioURL(
            int waitMs = Timeout.Infinite,
            CancellationTokenSource? cts = null)
        {
            Task? audioTask = null;

            try
            {
                cts ??= new();
                audioTask = Task.Run(() =>
                {
                    ObtainAudioURLInternal(cts);
                }, cts.Token);

                if (waitMs > 0)
                {
                    cts.CancelAfter(waitMs);
                    Task.Delay(1).Wait();
                    _ = audioTask.Wait(waitMs);
                }
                else
                {
                    audioTask.Wait();
                }

                if (string.IsNullOrWhiteSpace(AudioURL))
                {
                    throw new InvalidOperationException("Cannot get audio URL");
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                try
                {
                    cts?.Dispose();
                }
                catch { }
                try
                {
                    audioTask?.Dispose();
                }
                catch { }
            }
        }

        protected void ObtainAudioURLInternal(CancellationTokenSource cts);

        /// <summary>
        /// Reloads the corresponding API
        /// </summary>
        public void Reload()
        {
            ApiManager.ReloadApis(TrackType);
        }

        internal CompositeId GetCompositeId(string id)
        {
            return new(id, TrackType);
        }
    }
}
