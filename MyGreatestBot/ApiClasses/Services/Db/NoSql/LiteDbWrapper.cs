global using LiteDbWrapper = MyGreatestBot.ApiClasses.Services.Db.NoSql.LiteDbWrapper;
using LiteDB;
using MyGreatestBot.ApiClasses.ConfigClasses;
using MyGreatestBot.ApiClasses.ConfigClasses.JsonModels;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses;
using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using BotStorageCollection = LiteDB.ILiteCollection<MyGreatestBot.ApiClasses.Services.Db.NoSql.CollectionClasses.GenericDataValueDescriptor>;

namespace MyGreatestBot.ApiClasses.Services.Db.NoSql
{
    internal sealed class LiteDbWrapper : ITrackDatabaseAPI
    {
        private NoSqlDatabaseConfigJSON config;

        private LiteDatabase? LiteDbClient;
        private readonly object _queryLock = 0;

        ApiIntents IAPI.ApiType => ApiIntents.NoSql;
        ApiStatus IAPI.OldStatus { get; set; }

        private LiteDbWrapper()
        {
            config = ConfigManager.GetNoSqlDatabaseConfigJSON();
        }

        public static LiteDbWrapper Instance { get; private set; } = new();

        void IAPI.PerformAuthInternal()
        {
            LiteDbClient = new(@$"{config.DatabaseName}.db");
        }

        void IAPI.LogoutInternal()
        {
            LiteDbClient?.Dispose();
            LiteDbClient = null;
        }

        bool ITrackDatabaseAPI.IsTrackIgnored(BaseTrackInfo track, ulong guild)
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

        bool ITrackDatabaseAPI.IsAnyArtistIgnored(BaseTrackInfo track, ulong guild)
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

        void ITrackDatabaseAPI.AddIgnoredTrack(BaseTrackInfo track, ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.IgnoredTracksCollectionName);

                IEnumerable<GenericDataValueDescriptor> collection = querryCollection.FindAll();

                GenericDataValueKey key = GetTrackKey(track, guild);

                if (collection.FirstOrDefault(item => item.Key.Equals(key)) != null)
                {
                    throw new InvalidOperationException("Track already ignored");
                }

                _ = querryCollection.Insert(new GenericDataValueDescriptor(GetTrackKey(track, guild))
                {
                    HyperText = track.TrackName.ToString()
                });
            }
        }

        void ITrackDatabaseAPI.AddIgnoredArtist(BaseTrackInfo track, ulong guild, int index)
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

        void ITrackDatabaseAPI.SaveTracks(IEnumerable<BaseTrackInfo> tracks, ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.SavedTracksCollectionName);

                UInt128 position = 0;

                foreach (BaseTrackInfo track in tracks)
                {
                    _ = querryCollection.Insert(new GenericDataValueDescriptor(++position, GetTrackKey(track, guild))
                    {
                        HyperText = track.TrackName.ToString()
                    });
                }
            }
        }

        int ITrackDatabaseAPI.GetTracksCount(ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            int tracksCount;

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.SavedTracksCollectionName);

                _ = querryCollection.EnsureIndex(item => item.Key.GiuldId);

                IEnumerable<GenericDataValueDescriptor> collection =
                    querryCollection.Find(item => item.Key.GiuldId == guild);

                tracksCount = collection.Count();
            }

            return tracksCount;
        }

        List<CompositeId> ITrackDatabaseAPI.RestoreTracks(ulong guild)
        {
            if (LiteDbClient is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            List<CompositeId> result = [];

            lock (_queryLock)
            {
                BotStorageCollection querryCollection =
                    GetLiteCollection(DbCollectionNames.SavedTracksCollectionName);

                _ = querryCollection.EnsureIndex(item => item.Key.GiuldId);

                IEnumerable<GenericDataValueDescriptor> collection =
                    querryCollection.Find(item => item.Key.GiuldId == guild)
                    .OrderBy(item => item.Id);

                result.AddRange(collection.Select(item => new CompositeId(item.Key.GenericId, item.Key.ApiType)));
            }

            return result;
        }

        void ITrackDatabaseAPI.RemoveTracks(ulong guild)
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

        private static GenericDataValueKey GetTrackKey(BaseTrackInfo track, ulong guild)
        {
            return new(guild, track.TrackType, track.Id);
        }

        private static GenericDataValueKey GetArtistKey(BaseTrackInfo track, ulong guild, int index)
        {
            return new(guild, track.TrackType, track.ArtistArr[index].InnerId);
        }
    }
}
