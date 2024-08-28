using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Track information abstraction
    /// </summary>
    public abstract class BaseTrackInfo
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
            ApiManager.GetMusicApiInstance(TrackType)
            ?.Domains
            ?.ToString() ?? string.Empty;

        /// <summary>
        /// Extended track name
        /// </summary>
        public HyperLink TrackName { get; protected set; } = new("Title");

        /// <summary>
        /// Extended artists collection
        /// </summary>
        public HyperLink[] ArtistArr { get; protected set; } = [new("Artist")];

        /// <summary>
        /// Extended album name
        /// </summary>
        [AllowNull] public HyperLink AlbumName { get; protected set; }

        /// <summary>
        /// Extended playlist name
        /// </summary>
        [AllowNull] public HyperLink PlaylistName { get; protected set; }

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
        public TimeSpan Duration { get; protected set; }

        /// <summary>
        /// Current track time position
        /// </summary>
        public TimeSpan TimePosition { get; protected set; }

        /// <summary>
        /// Thumbnails image URL collection
        /// </summary>
        [AllowNull] public IEnumerable<string?> CoverUrlCollection { get; protected set; }

        /// <summary>
        /// Audio URL for FFMPEG
        /// </summary>
        public string AudioURL { get; protected set; } = string.Empty;

        /// <summary>
        /// Indicates when a new track should be added, if possible.
        /// </summary>
        public bool Radio { get; set; }

        /// <summary>
        /// Indicates when a blacklist check should not be performed.
        /// </summary>
        public bool BypassCheck { get; set; }

        /// <summary>
        /// Does the track have a duration.
        /// </summary>
        public bool IsLiveStream => Duration == TimeSpan.Zero;

        /// <summary>
        /// Track cover image.
        /// </summary>
        private DiscordEmbedBuilder.EmbedThumbnail? LastThumbnail { get; set; }

        /// <summary>
        /// Check for seek operation possible.
        /// </summary>
        /// <param name="span">Specified position.</param>
        /// <returns>True if possible to seek, otherwise false.</returns>
        public bool IsRewindPossible(TimeSpan span)
        {
            return !(IsLiveStream || span > Duration);
        }

        /// <summary>
        /// Performs seek operation with check.
        /// </summary>
        /// <param name="span">Specified position.</param>
        public void PerformRewind(TimeSpan span)
        {
            if (IsRewindPossible(span))
            {
                TimePosition = span;
            }
        }

        /// <summary>
        /// Unused method. Needed for doxygen.
        /// </summary>
        /// <param name="prefix">For example "Playing".</param>
        /// <returns>Content string.</returns>
        [SuppressMessage("CodeQuality", "IDE0052")]
        private string GetMessageDoxygen(string prefix)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get Discord message content.
        /// </summary>
        /// <param name="prefix">
        /// <inheritdoc cref="GetMessageDoxygen" path="/param[@name='prefix']"/>
        /// </param>
        /// <returns>
        /// <inheritdoc cref="GetMessageDoxygen"></inheritdoc>
        /// </returns>
        public string GetMessage(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "Empty prefix";
            }

            string result = string.Empty;

            result += $"{prefix}: {TrackName}{Environment.NewLine}";
            result += $"Author: {string.Join(", ", ArtistArr.Select(a => a.ToString()))}";

            if (!IsLiveStream)
            {
                result += $"{Environment.NewLine}Duration: {Duration.GetCustomTime()}";
            }

            if (!string.IsNullOrWhiteSpace(AlbumName?.Title))
            {
                result += $"{Environment.NewLine}Album: {AlbumName}";
            }

            if (!string.IsNullOrWhiteSpace(PlaylistName?.Title))
            {
                result += $"{Environment.NewLine}Playlist: {PlaylistName}";
            }

            if (TimePosition != TimeSpan.Zero)
            {
                result += $"{Environment.NewLine}Time: {TimePosition.GetCustomTime()}";
            }

            return result;
        }

        /// <summary>
        /// Get console message string.
        /// </summary>
        /// <param name="prefix">
        /// <inheritdoc cref="GetMessageDoxygen" path="/param[@name='prefix']"/>
        /// </param>
        /// <returns>
        /// <inheritdoc cref="GetMessageDoxygen"></inheritdoc>
        /// </returns>
        public string GetShortMessage(string prefix)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "Empty prefix";
            }

            return $"{prefix}: {Title} by {string.Join(", ", ArtistArr.Select(a => a.Title))}";
        }

        /// <summary>
        /// Get Discord thumbnail with track cover image.
        /// </summary>
        /// <returns>Track cover image as thumbnail.</returns>
        public DiscordEmbedBuilder.EmbedThumbnail? GetThumbnail()
        {
            if (LastThumbnail != null)
            {
                return LastThumbnail;
            }

            if (CoverUrlCollection == null)
            {
                return LastThumbnail;
            }

            List<string> collection = CoverUrlCollection
                .Where(v => !string.IsNullOrWhiteSpace(v))
                .Cast<string>()
                .ToList();

            int i = 0;

            while (i < collection.Count)
            {
                string url = collection[i];

                if (IAccessible.IsUrlSuccess(url, false))
                {
                    LastThumbnail = new()
                    {
                        Url = url
                    };

                    break;
                }

                i++;
            }

            return LastThumbnail;
        }

        /// <summary>
        /// Retrieves the audio URL.
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

        protected abstract void ObtainAudioURLInternal(CancellationTokenSource cts);

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
