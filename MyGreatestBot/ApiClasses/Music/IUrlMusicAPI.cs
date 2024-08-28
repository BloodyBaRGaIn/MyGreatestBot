using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.Music
{
    public interface IUrlMusicAPI : IMusicAPI
    {
        /// <summary>
        /// Get tracks from URL
        /// </summary>
        /// <param name="url">URL</param>
        /// <returns>Track collection</returns>
        IEnumerable<BaseTrackInfo>? GetTracksFromUrl(string url);
    }
}
