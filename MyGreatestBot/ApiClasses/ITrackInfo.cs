using MyGreatestBot.ApiClasses.Music.Spotify;
using MyGreatestBot.ApiClasses.Music.Vk;
using MyGreatestBot.ApiClasses.Music.Yandex;
using MyGreatestBot.ApiClasses.Music.Youtube;
using MyGreatestBot.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Runtime.Versioning;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Track information abstraction
    /// </summary>
    [SupportedOSPlatform("windows")]
    public interface ITrackInfo
    {
        /// <summary>
        /// Track type
        /// </summary>
        public virtual ApiIntents TrackType => ApiIntents.None;

        /// <summary>
        /// Base URL
        /// </summary>
        public virtual string Domain => ApiManager.DoaminsDictionary[TrackType];

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
        [AllowNull]
        public HyperLink AlbumName { get; }

        /// <summary>
        /// Extended playlist name
        /// </summary>
        [AllowNull]
        public HyperLink PlaylistName { get; }

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
        public TimeSpan Seek { get; protected set; }

        /// <summary>
        /// Thumbnails image URL
        /// </summary>
        [AllowNull]
        public string CoverURL { get; }

        /// <summary>
        /// Audio URL for FFMPEG
        /// </summary>
        public string AudioURL { get; }

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
                Seek = span;
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
                result += $"{Environment.NewLine}Duration: {Duration:hh\\:mm\\:ss}";
            }

            if (AlbumName != null && !string.IsNullOrWhiteSpace(AlbumName.Title))
            {
                result += $"{Environment.NewLine}Album: {AlbumName}";
            }

            if (PlaylistName != null && !string.IsNullOrWhiteSpace(PlaylistName.Title))
            {
                result += $"{Environment.NewLine} Playlist: {PlaylistName}";
            }

            if (Seek != TimeSpan.Zero)
            {
                result += $"{Environment.NewLine} Time: {Seek:hh\\:mm\\:ss}";
            }

            return result;
        }

        public string GetShortMessage()
        {
            return $"Playing: {Title} by {string.Join(", ", ArtistArr.Select(a => a.Title))}";
        }

        /// <summary>
        /// Get Discord message thumbnail with track cover image
        /// </summary>
        /// <returns>Track cover image as thumbnail</returns>
        [return: MaybeNull]
        public DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail GetThumbnail()
        {
            return string.IsNullOrWhiteSpace(CoverURL)
                ? null
                : new DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail()
                {
                    Url = CoverURL
                };
        }

        /// <summary>
        /// Retrieves the audio URL
        /// </summary>
        public void ObtainAudioURL();

        /// <summary>
        /// Reloads the corresponding API
        /// </summary>
        public void Reload()
        {
            ApiManager.ReloadApis(TrackType);
        }

        /// <summary>
        /// Arguments string for FFMPEG
        /// </summary>
        public string Arguments => $"-loglevel fatal {(Seek == TimeSpan.Zero || IsLiveStream ? "" : $"-ss {Seek} ")}" +
                                   $"-i \"{AudioURL}\" -f s16le -ac 2 -ar 48000 -filter:a \"volume = 0.25\" pipe:1";

        /// <summary>
        /// Tracks comparsion method
        /// </summary>
        /// <param name="other">Other track info instance</param>
        /// <returns>Zero if fully equals</returns>
        public int CompareTo([AllowNull] ITrackInfo other)
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

        internal static ITrackInfo? GetTrack(ApiIntents api, string id)
        {
            return api switch
            {
                ApiIntents.Youtube => YoutubeApiWrapper.GetTrack(id),
                ApiIntents.Yandex => YandexApiWrapper.GetTrack(id),
                ApiIntents.Vk => VkApiWrapper.GetTrack(id),
                ApiIntents.Spotify => SpotifyApiWrapper.GetTrack(id),
                _ => null,
            };
        }
    }
}
