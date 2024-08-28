using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.Music
{
    public interface ITextMusicAPI : IMusicAPI
    {
        /// <summary>
        /// Get tracks from query string
        /// </summary>
        /// <param name="text">Query string</param>
        /// <returns>Track collection</returns>
        IEnumerable<BaseTrackInfo>? GetTracksFromPlainText(string text);
    }
}
