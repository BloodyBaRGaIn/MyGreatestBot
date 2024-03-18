namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs with search support
    /// </summary>
    public interface ISearchable : IMusicAPI
    {
        /// <summary>
        /// Search track with other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITrackInfo? SearchTrack(ITrackInfo other);
    }
}
