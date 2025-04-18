﻿using DSharpPlus.Entities;
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
    /// Track information abstrac class.
    /// </summary>
    public abstract class BaseTrackInfo : IEquatable<BaseTrackInfo>
    {
        /// <summary>
        /// Track type
        /// </summary>
        public abstract ApiIntents TrackType { get; }

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
        /// Discord thumbnail with track cover image.
        /// </summary>
        private DiscordEmbedBuilder.EmbedThumbnail? LastThumbnail;

        /// <inheritdoc cref="LastThumbnail"/>
        public DiscordEmbedBuilder.EmbedThumbnail? Thumbnail
        {
            get
            {
                if (LastThumbnail != null
                    || CoverUrlCollection == null
                    || !CoverUrlCollection.Any())
                {
                    return LastThumbnail;
                }

                foreach (string? url in CoverUrlCollection)
                {
                    if (!string.IsNullOrWhiteSpace(url) && IAccessible.IsUrlSuccess(url, false))
                    {
                        LastThumbnail = new()
                        {
                            Url = url
                        };

                        break;
                    }
                }

                return LastThumbnail;
            }
        }

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
        /// Get Discord message content.
        /// </summary>
        /// <param name="prefix">
        /// Message prefix string.<br/>
        /// For example:<br/>
        /// <code>"Playing"</code>
        /// </param>
        /// <returns>Content string.</returns>
        public string GetMessage(string prefix, bool shortMessage = false)
        {
            if (string.IsNullOrWhiteSpace(prefix))
            {
                prefix = "Blank prefix";
            }

            string caption = $"{prefix}: ";

            if (shortMessage)
            {
                caption += $"{Title} by " +
                    $"{string.Join(", ", ArtistArr.Select(static a => a.Title))}";

                return caption;
            }

            caption += $"{TrackName}";

            string authors = $"Author: {string.Join(", ", ArtistArr.Select(static a => a.ToString()))}";

            string? album = string.IsNullOrWhiteSpace(AlbumName?.Title)
                ? null
                : $"Album: {AlbumName}";

            string? playlist = string.IsNullOrWhiteSpace(PlaylistName?.Title)
                ? null
                : $"Playlist: {PlaylistName}";

            string? duration = IsLiveStream
                ? null
                : $"Duration: {Duration.GetCustomTime()}";

            string? time = TimePosition == TimeSpan.Zero
                ? null
                : $"Time: {TimePosition.GetCustomTime()}";

            return string.Join(Environment.NewLine,
                               StringExtensions.EnsureStrings(
                                   caption, authors, album, playlist, duration, time));
        }

        /// <summary>
        /// Retrieves the audio URL.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public void ObtainAudioURL(
            int waitMs = Timeout.Infinite,
            CancellationTokenSource? cts = null)
        {
            Exception? internalException = null;
            cts ??= new();

            Task audioTask = Task.Run(() =>
            {
                try
                {
                    ObtainAudioURLInternal(cts);
                }
                catch (Exception ex)
                {
                    internalException = ex;
                }
            }, cts.Token);

            try
            {
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

                if (internalException != null)
                {
                    throw internalException;
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

        public bool Equals(BaseTrackInfo? other)
        {
            return TrackName.InnerId.Equals(other?.TrackName.InnerId);
        }

        public override bool Equals([AllowNull] object obj)
        {
            return Equals(obj as BaseTrackInfo);
        }

        public override int GetHashCode()
        {
            return TrackName.InnerId.GetHashCode();
        }
    }
}
