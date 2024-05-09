using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs
    /// </summary>
    public interface IMusicAPI : IAPI, IAccessible
    {
        /// <summary>
        /// Get tracks from URL
        /// </summary>
        /// <param name="query">URL</param>
        /// <returns>Track collection</returns>
        IEnumerable<ITrackInfo>? GetTracks(string query);

        /// <summary>
        /// Get track from ID
        /// </summary>
        /// <param name="id">Track identificator</param>
        /// <param name="time">Track starting time in seconds (should be not negative)</param>
        /// <returns>Track</returns>
        ITrackInfo? GetTrack(string id, int time = 0);
    }

    public interface IQueryMusicAPI : IMusicAPI
    {
        /// <summary>
        /// Get tracks from query string
        /// </summary>
        /// <param name="text">Query string</param>
        /// <returns>Track collection</returns>
        IEnumerable<ITrackInfo>? GetTracksFromPlainText(string text);
    }
}
