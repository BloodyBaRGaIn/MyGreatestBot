﻿using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses
{
    /// <summary>
    /// Interface for tracks and artists database
    /// </summary>
    public interface ITrackDatabaseAPI : IAPI
    {
        /// <summary>
        /// Unused method. Needed for doxygen.
        /// </summary>
        /// <param name="track">Track instance</param>
        /// <param name="guild">Discord giuld ID</param>
        /// <returns>True if the track is in the collection, otherwise returns false</returns>
        protected bool Doxygen(ITrackInfo track, ulong guild)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Checks that the track is in the collection of ignored tracks
        /// </summary>
        /// <param name="track"><inheritdoc cref="Doxygen" path="/param[@name='track']"/></param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        /// <returns>True if the track is in the collection, otherwise returns false</returns>
        bool IsTrackIgnored(ITrackInfo track, ulong guild);

        /// <summary>
        /// Checks that at least one of the track's artists is in the collection of ignored artists
        /// </summary>
        /// <param name="track"><inheritdoc cref="Doxygen" path="/param[@name='track']"/></param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        /// <returns><inheritdoc cref="IsTrackIgnored"/></returns>
        bool IsAnyArtistIgnored(ITrackInfo track, ulong guild);

        /// <summary>
        /// Adds the track to the ignored tracks collection
        /// </summary>
        /// <param name="track"><inheritdoc cref="Doxygen" path="/param[@name='track']"/></param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        void AddIgnoredTrack(ITrackInfo track, ulong guild);

        /// <summary>
        /// Adds all of the track's artists to the ignored arists collection
        /// </summary>
        /// <param name="track"><inheritdoc cref="Doxygen" path="/param[@name='track']"/></param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        public virtual void AddIgnoredArtist(ITrackInfo track, ulong guild)
        {
            for (int i = 0; i < track.ArtistArr.Length; i++)
            {
                AddIgnoredArtist(track, guild, i);
            }
        }

        /// <summary>
        /// Adds the track's artist to the ignored artists collection by specified index
        /// </summary>
        /// <param name="track"><inheritdoc cref="Doxygen" path="/param[@name='track']"/></param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        /// <param name="index">Zero-based artist index</param>
        void AddIgnoredArtist(ITrackInfo track, ulong guild, int index);

        /// <summary>
        /// Saves the tracks collection to the database
        /// </summary>
        /// <param name="tracks">Tracks collection to be saved</param>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        void SaveTracks(IEnumerable<ITrackInfo> tracks, ulong guild);

        /// <summary>
        /// Restores tracks collection from database
        /// </summary>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        /// <returns>Collection of API type and track ID tuples</returns>
        List<CompositeId> RestoreTracks(ulong guild);

        /// <summary>
        /// Removes all saved tracks
        /// </summary>
        /// <param name="guild"><inheritdoc cref="Doxygen" path="/param[@name='guild']"/></param>
        void RemoveTracks(ulong guild);
    }
}
