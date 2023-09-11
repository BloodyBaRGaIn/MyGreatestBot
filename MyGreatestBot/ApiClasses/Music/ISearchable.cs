namespace MyGreatestBot.ApiClasses.Music
{
    public interface ISearchable : IAPI, IAccessible
    {
        /// <summary>
        /// Search track with other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITrackInfo? SearchTrack(ITrackInfo other);
    }
}
