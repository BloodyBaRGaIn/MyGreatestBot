namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs with radio mode support
    /// </summary>
    public interface IRadioAPI : IMusicAPI
    {
        /// <summary>
        /// Get track in radio mode
        /// </summary>
        /// <param name="id">Previous track ID</param>
        /// <returns>Track</returns>
        ITrackInfo? GetRadio(string id);
    }
}
