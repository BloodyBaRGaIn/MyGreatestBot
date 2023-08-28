using DicordNET.Utils;
using System;
using System.Linq;
using System.Runtime.Versioning;

namespace DicordNET.ApiClasses
{
    /// <summary>
    /// Track information abstraction
    /// </summary>
    [SupportedOSPlatform("windows")]
    internal interface ITrackInfo
    {
        /// <summary>
        /// This instance
        /// </summary>
        protected ITrackInfo Base { get; }

        /// <summary>
        /// Base URL
        /// </summary>
        protected string Domain { get; }

        /// <summary>
        /// Track type
        /// </summary>
        internal ApiIntents TrackType { get; }

        /// <summary>
        /// Track ID
        /// </summary>
        internal string Id { get; }

        /// <summary>
        /// Extended track name
        /// </summary>
        internal HyperLink TrackName { get; }

        /// <summary>
        /// Extended artists collection
        /// </summary>
        internal HyperLink[] ArtistArr { get; }

        /// <summary>
        /// Extended album name
        /// </summary>
        internal HyperLink? AlbumName { get; }

        /// <summary>
        /// Extended playlist name
        /// </summary>
        internal HyperLink? PlaylistName { get; }

        /// <summary>
        /// Base track name
        /// </summary>
        internal string Title { get; }

        /// <summary>
        /// Track duration
        /// </summary>
        internal TimeSpan Duration { get; }

        /// <summary>
        /// Current track time position
        /// </summary>
        protected internal TimeSpan Seek { get; protected set; }

        /// <summary>
        /// Thumbnails image URL
        /// </summary>
        internal string? CoverURL { get; }

        /// <summary>
        /// Audio URL for FFMPEG
        /// </summary>
        internal string AudioURL { get; }

        /// <summary>
        /// Is a stream
        /// </summary>
        internal bool IsLiveStream { get; }

        /// <summary>
        /// Check for seek operation possible
        /// </summary>
        /// <param name="span">Specified position</param>
        /// <returns>True if possible to seek</returns>
        internal bool TrySeek(TimeSpan span)
        {
            if (IsLiveStream || Duration == TimeSpan.Zero || span > Duration)
            {
                return false;
            }

            //Seek = span;

            return true;
        }

        /// <summary>
        /// Performs seek operation with check
        /// </summary>
        /// <param name="span">Specified position</param>
        internal void PerformSeek(TimeSpan span)
        {
            if (TrySeek(span))
            {
                Seek = span;
            }
        }

        /// <summary>
        /// Get Discord message content
        /// </summary>
        /// <returns>Content string</returns>
        internal string GetMessage()
        {
            string result = string.Empty;
            result += $"Playing: {TrackName}\n";
            result += "Author: ";

            result += string.Join(", ", ArtistArr.Select(a => a.ToString()));
            result += '\n';

            if (Duration != TimeSpan.Zero)
            {
                result += $"Duration: {Duration:hh\\:mm\\:ss}\n";
            }

            if (AlbumName != null)
            {
                if (!string.IsNullOrWhiteSpace(AlbumName.Title))
                {
                    result += $"Album: {AlbumName}\n";
                }
            }

            if (PlaylistName != null)
            {
                if (!string.IsNullOrWhiteSpace(PlaylistName.Title))
                {
                    result += $"Playlist: {PlaylistName}\n";
                }
            }

            if (Seek != TimeSpan.Zero)
            {
                result += $"Time: {Seek:hh\\:mm\\:ss}\n";
            }

            return result.Trim('\n');
        }

        /// <summary>
        /// Get Discord message thumbnail with track cover image
        /// </summary>
        /// <returns>Track cover image as thumbnail</returns>
        internal DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail? GetThumbnail()
        {
            if (string.IsNullOrWhiteSpace(CoverURL))
            {
                return null;
            }

            return new DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail()
            {
                Url = CoverURL
            };
        }

        /// <summary>
        /// Retrieves the audio URL
        /// </summary>
        internal abstract void ObtainAudioURL();

        /// <summary>
        /// Reloads the corresponding API
        /// </summary>
        internal abstract void Reload();

        /// <summary>
        /// Arguments string for FFMPEG
        /// </summary>
        internal string Arguments => $"-loglevel fatal {(Seek == TimeSpan.Zero || IsLiveStream ? "" : $"-ss {Seek} ")}" +
                                     $"-i \"{AudioURL}\" -f s16le -ac 2 -ar 48000 -filter:a \"volume = 0.25\" pipe:1";

        /// <summary>
        /// Tracks comparsion method
        /// </summary>
        /// <param name="other">Other track info instance</param>
        /// <returns>Zero if fully equals</returns>
        internal int CompareTo(ITrackInfo? other)
        {
            System.Numerics.BigInteger result = 0;
            if (this is null || other is null)
            {
                return int.MaxValue;
            }

            int name = other.TrackName.CompareTo(TrackName);
            int album;
            if (other.AlbumName is null)
            {
                album = int.MaxValue;
            }
            else
            {
                album = other.AlbumName.CompareTo(AlbumName);
            }
            int artist = 0;
            if (other.ArtistArr.Length != ArtistArr.Length)
            {
                artist = int.MaxValue;
            }
            else
            {
                for (int i = 0; i < ArtistArr.Length; i++)
                {
                    artist += other.ArtistArr[i].CompareTo(ArtistArr[i]);
                }
            }

            result += name;
            result += album;
            result += artist;

            if (result > int.MaxValue)
            {
                return int.MaxValue;
            }

            if (result < int.MinValue)
            {
                return int.MinValue;
            }

            return (int)result;
        }
    }
}
