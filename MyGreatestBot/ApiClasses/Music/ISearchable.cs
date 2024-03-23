using System;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs with search support
    /// </summary>
    public interface ISearchable : IMusicAPI
    {
        public static readonly TimeSpan MaximumTimeDifference = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Search track with other
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        ITrackInfo? SearchTrack(ITrackInfo other);
    }
}
