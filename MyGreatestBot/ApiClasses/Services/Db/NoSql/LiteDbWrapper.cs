using LiteDB;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MyGreatestBot.ApiClasses.Services.Db.NoSql
{
    using BotStorageCollection = ILiteCollection<GenericDataValueDescriptor>;

    internal class LiteDbWrapper : ITrackDatabaseAPI
    {
        private NoSqlDatabaseConfigJSON config;

        private LiteDatabase? LiteDbClient;
        private readonly object _queryLock = 0;

        ApiIntents IAPI.ApiType => ApiIntents.NoSql;

        private LiteDbWrapper()
        {
            config = ConfigManager.GetNoSqlDatabaseConfigJSON();
        }

        public static LiteDbWrapper Instance { get; private set; } = new();

        public void PerformAuth()
        {
            LiteDbClient = new(@$"{config.DatabaseName}.db");
        }

        public void Logout()
        {
            LiteDbClient?.Dispose();
            LiteDbClient = null;
        }

        public bool IsTrackIgnored(ITrackInfo track, ulong guild)
        {
            if (LiteDbClient is null)
            {
                return false;
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.IgnoredTracksCollectionName);

                IEnumerable<GenericDataValueDescriptor> collection = querryCollection.FindAll();

                GenericDataValueKey key = GetTrackKey(track, guild);

                return collection.FirstOrDefault(item => item.Key.Equals(key)) != null;
            }
        }

        public bool IsAnyArtistIgnored(ITrackInfo track, ulong guild)
        {
            if (LiteDbClient is null)
            {
                return false;
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.IgnoredArtistsCollectionName);

                IEnumerable<GenericDataValueDescriptor> collection = querryCollection.FindAll();

                for (int i = 0; i < track.ArtistArr.Length; i++)
                {
                    GenericDataValueKey key = GetArtistKey(track, guild, i);
                    if (collection.FirstOrDefault(item => item.Key.Equals(key)) != null)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void AddIgnoredTrack(ITrackInfo track, ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            if (IsTrackIgnored(track, guild))
            {
                throw new InvalidOperationException("Track already ignored");
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.IgnoredTracksCollectionName);

                _ = querryCollection.Insert(new GenericDataValueDescriptor(GetTrackKey(track, guild))
                {
                    HyperText = track.TrackName.ToString()
                });
            }
        }

        public void AddIgnoredArtist(ITrackInfo track, ulong guild, int index)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.IgnoredArtistsCollectionName);

                GenericDataValueKey key = GetArtistKey(track, guild, index);

                IEnumerable<GenericDataValueDescriptor> collection = querryCollection.FindAll();

                if (collection.FirstOrDefault(item => item.Key.Equals(key)) != null)
                {
                    throw new InvalidOperationException("Artist already ignored");
                }

                _ = querryCollection.Insert(new GenericDataValueDescriptor(key)
                {
                    HyperText = track.ArtistArr[index].ToString()
                });
            }
        }

        public void SaveTracks(IEnumerable<ITrackInfo> tracks, ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.SavedTracksCollectionName);

                foreach (ITrackInfo track in tracks)
                {
                    _ = querryCollection.Insert(new GenericDataValueDescriptor(GetTrackKey(track, guild))
                    {
                        HyperText = track.TrackName.ToString()
                    });
                }
            }
        }

        public List<(ApiIntents, string)> RestoreTracks(ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            List<(ApiIntents, string)> result = [];

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.SavedTracksCollectionName);

                IEnumerable<GenericDataValueDescriptor> collection =
                    querryCollection.Find(item => item.Key.GiuldId == guild);

                result.AddRange(collection.Select(item => (item.Key.ApiType, item.Key.GenericId)));
            }

            return result;
        }

        public void RemoveTracks(ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            _ = LiteDbClient.DropCollection(DbCollectionNames.SavedTracksCollectionName);
        }

        private BotStorageCollection GetLiteCollection(string name)
        {
            return LiteDbClient is null
                ? throw new InvalidOperationException("DB connection not initialized")
                : LiteDbClient.GetCollection<GenericDataValueDescriptor>(name);
        }

        private static GenericDataValueKey GetTrackKey(ITrackInfo track, ulong guild)
        {
            return new(guild, track.TrackType, track.Id);
        }

        private static GenericDataValueKey GetArtistKey(ITrackInfo track, ulong guild, int index)
        {
            return new(guild, track.TrackType, track.ArtistArr[index].InnerId);
        }
    }
}
