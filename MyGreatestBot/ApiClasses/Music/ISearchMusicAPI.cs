using System;

namespace MyGreatestBot.ApiClasses.Music
{
    /// <summary>
    /// Interface for music APIs with search support
    /// </summary>
    public interface ISearchMusicAPI : IMusicAPI
    {
        static readonly TimeSpan MaximumTimeDifference = TimeSpan.FromSeconds(2);
        /// <summary>
        /// Search track with other
        /// </summary>
        /// <param name="other">Track with desired info</param>
        /// <returns>Track from desired platform</returns>
        BaseTrackInfo? SearchTrack(BaseTrackInfo other);
    }
}
