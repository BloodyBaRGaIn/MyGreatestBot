using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

using EmbedThumbnail = DSharpPlus.Entities.DiscordEmbedBuilder.EmbedThumbnail;

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
        public virtual string Domain => ApiManager.Get<IMusicAPI>(TrackType)?.Domains?.ToString() ?? string.Empty;

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
        public TimeSpan TimePosition { get; protected set; }

        /// <summary>
        /// Thumbnails image URL
        /// </summary>
        [AllowNull]
        public string CoverURL { get; }

        /// <summary>
        /// Audio URL for FFMPEG
        /// </summary>
        [DisallowNull]
        public string AudioURL { get; }

        public bool Radio { get; set; }
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

        private static string GetCustomTime(TimeSpan time)
        {
            static string GetPaddedValue(double x)
            {
                return $"{(int)x}".PadLeft(2, '0');
            }

            return $"{GetPaddedValue(time.TotalHours)}:{GetPaddedValue(time.Minutes)}:{GetPaddedValue(time.Seconds)}";
        }

        public string GetShortMessage(string prefix)
        {
            return $"{prefix}{Title} by {string.Join(", ", ArtistArr.Select(a => a.Title))}";
        }

        /// <summary>
        /// Get Discord message thumbnail with track cover image
        /// </summary>
        /// <returns>Track cover image as thumbnail</returns>
        public EmbedThumbnail? GetThumbnail()
        {
            return string.IsNullOrWhiteSpace(CoverURL)
                ? null
                : new EmbedThumbnail()
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
        public string Arguments => $"-loglevel error {(TimePosition == TimeSpan.Zero || IsLiveStream ? "" : $"-ss {TimePosition} ")}" +
                                   $"-i \"{AudioURL}\" -f s16le -ac 2 -ar 48000 -filter:a \"volume = 0.25\" pipe:1";

        /// <summary>
        /// Get track from search result by its id
        /// </summary>
        /// <param name="api">API flag </param>
        /// <param name="id"></param>
        /// <returns></returns>
        internal static ITrackInfo? GetTrack(ApiIntents api, string id)
        {
            return api switch
            {
                ApiIntents.Youtube => Youtube.YoutubeApiWrapper.MusicInstance.GetTrack(id),
                ApiIntents.Yandex => Yandex.YandexApiWrapper.MusicInstance.GetTrack(id),
                ApiIntents.Vk => Vk.VkApiWrapper.MusicInstance.GetTrack(id),
                ApiIntents.Spotify => Spotify.SpotifyApiWrapper.MusicInstance.GetTrack(id),
                _ => null,
            };
        }

        internal CompositeId GetCompositeId(string id)
        {
            return new(id, TrackType);
        }
    }
}
