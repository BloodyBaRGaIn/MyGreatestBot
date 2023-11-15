using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs
    /// </summary>
    public interface IMusicAPI : IAPI, IAccessible
    {
        /// <summary>
        /// Get tracks from query
        /// </summary>
        /// <param name="query">URL</param>
        /// <returns></returns>
        IEnumerable<ITrackInfo> GetTracks(string query);

        IEnumerable<ITrackInfo> GetTracksSearch(string query);

        /// <summary>
        /// Get track from its ID
        /// </summary>
        /// <param name="id">Track identificator</param>
        /// <returns></returns>
        ITrackInfo? GetTrack(string id);
    }
}
