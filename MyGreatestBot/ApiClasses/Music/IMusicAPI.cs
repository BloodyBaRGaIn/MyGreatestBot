namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs
    /// </summary>
    public interface IMusicAPI : IAPI, IAccessible
    {
        /// <summary>
        /// Get track from ID
        /// </summary>
        /// <param name="id">Track identificator</param>
        /// <param name="time">
        /// Track starting time in seconds (should be not negative).<br/>
        /// This parameter is optional.</param>
        /// <returns>Track</returns>
        ITrackInfo? GetTrackFromId(string id, int time = 0);
    }
}
