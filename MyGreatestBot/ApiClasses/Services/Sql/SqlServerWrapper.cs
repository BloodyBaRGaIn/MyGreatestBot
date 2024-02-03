using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.ApiClasses.Services.Sql.TableClasses;
using MyGreatestBot.ApiClasses.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Sql
{
    [SupportedOSPlatform("windows")]
    public sealed class SqlServerWrapper : IAPI
    {
        private readonly string ServerString;
        private readonly string ConnectionString;

        private readonly IgnoredTracksTable IgnoredTracks;
        private readonly IgnoredArtistsTable IgnoredArtists;
        private readonly SavedTracksTable TrackInfo;

        private SqlDatabaseConfigJSON config;

        private readonly DatabaseScriptProvider scriptProvider;

        private SqlConnection? _connection;

        private readonly object _queryLock = 0;
        private readonly object _connectionLock = 0;

        private SqlServerWrapper()
        {
            try
            {
                config = ConfigManager.GetSqlDatabaseConfigJSON();

                scriptProvider = new(config.LocalDirectory, config.DatabaseName);

                ServerString = new ConnectionStringBuilder(config.ServerName).Build();
                ConnectionString = new ConnectionStringBuilder(config.ServerName, config.DatabaseName).Build();

                IgnoredTracks = new(config.DatabaseName);
                IgnoredArtists = new(config.DatabaseName);
                TrackInfo = new(config.DatabaseName);
            }
            catch (Exception ex)
            {
                throw new SqlApiException("Initialization failed", ex);
            }
        }

        public static SqlServerWrapper Instance { get; private set; } = new();

        ApiIntents IAPI.ApiType => ApiIntents.Sql;

        public void PerformAuth()
        {
            lock (_connectionLock)
            {
                SqlServiceWrapper.Run();

                while (true)
                {
                    _connection = new(ConnectionString);
                    try
                    {
                        _connection.Open();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (SqlServiceWrapper.Run(ex.Number))
                        {
                            continue;
                        }
                        switch (ex.Number)
                        {
                            // Cannot open database
                            case 4060:
                                _connection = new(ServerString);
                                Server server = new(new ServerConnection(_connection));
                                _ = server.ConnectionContext.ExecuteNonQuery(scriptProvider.GetDatabaseScript());
                                break;

                            default:
                                Console.WriteLine(ex.Number);
                                throw;
                        }
                    }
                }
            }
        }

        public void Logout()
        {
            lock (_connectionLock)
            {
                _connection?.Close();
            }
        }

        private void Reopen()
        {
            Logout();
            PerformAuth();
        }

        internal bool IsTrackIgnored(ITrackInfo track, ulong guild)
        {
            lock (_queryLock)
            {
                return HasRowsGeneric(IgnoredTracks, guild, (int)track.TrackType, track.Id);
            }
        }

        internal bool IsAnyArtistIgnored(ITrackInfo track, ulong guild)
        {
            lock (_queryLock)
            {
                return IsAnyArtistIgnored(guild, (int)track.TrackType, track.ArtistArr.Select(a => a.InnerId));
            }
        }

        internal void AddIgnoredTrack(ITrackInfo track, ulong guild)
        {
            lock (_queryLock)
            {
                InsertGeneric(IgnoredTracks, guild, (int)track.TrackType, track.Id, track.TrackName.ToString());
            }
        }

        internal void AddIgnoredArtist(ITrackInfo track, int index, ulong guild)
        {
            HyperLink artist = track.ArtistArr[index];
            lock (_queryLock)
            {
                InsertGeneric(IgnoredArtists, guild, (int)track.TrackType, artist.InnerId, artist.ToString());
            }
        }

        internal void SaveTracks(IEnumerable<ITrackInfo> tracks, ulong guild)
        {
            lock (_queryLock)
            {
                foreach (ITrackInfo track in tracks)
                {
                    InsertGeneric(TrackInfo, guild, (int)track.TrackType, track.Id, track.TrackName.ToString());
                }
            }
        }

        internal List<(ApiIntents, string)> RestoreTracks(ulong guild)
        {
            List<(ApiIntents, string)> items = [];
            lock (_queryLock)
            {
                try
                {
                    Reopen();
                }
                catch
                {
                    return items;
                }

                SqlDataReader? reader = null;

                while (true)
                {
                    SqlCommand commandSelect = TrackInfo.GetSelectQuery(_connection, guild);
                    try
                    {
                        reader = commandSelect.ExecuteReader();
                        break;
                    }
                    catch (SqlException ex)
                    {
                        if (SqlServiceWrapper.Run(ex.Number)
                            || EnsureTableCreated(TrackInfo, ex.Number))
                        {
                            continue;
                        }
                        switch (ex.Number)
                        {
                            default:
                                Console.WriteLine(ex.Number);
                                throw;
                        }
                    }
                }

                if (reader == null)
                {
                    return items;
                }

                while (reader.Read())
                {
                    _ = Task.Yield();
                    string type = reader["Type"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(type))
                    {
                        continue;
                    }
                    string id = reader["ID"]?.ToString() ?? string.Empty;
                    if (string.IsNullOrWhiteSpace(id))
                    {
                        continue;
                    }
                    id = id.TrimEnd();
                    items.Add(((ApiIntents)Enum.ToObject(typeof(ApiIntents), int.Parse(type)), id));
                }
                reader.Close();
                _ = Task.Delay(1);
                return items;
            }
        }

        internal void RemoveTracks(ulong guild)
        {
            lock (_queryLock)
            {
                try
                {
                    DeleteGeneric(TrackInfo, guild);
                    SqlCommand command = new($"DBCC CHECKIDENT ('[{TrackInfo.Name}]', RESEED, 0)", _connection);
                    _ = command.ExecuteNonQuery();
                }
                catch
                {
                    ;
                }
            }
        }

        private bool IsAnyArtistIgnored(ulong guild, int type, IEnumerable<string> ids)
        {
            if (_connection is null || ids is null || !ids.Any())
            {
                return false;
            }

            try
            {
                return ids.Any(id => HasRowsGeneric(IgnoredArtists, guild, type, id));
            }
            catch
            {
                throw;
            }
        }

        private bool HasRowsGeneric(GenericTable table, ulong guild, int type, string id)
        {
            if (_connection is null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            Reopen();

            while (true)
            {
                SqlCommand command = table.GetSelectWhereQuery(_connection, guild, type, id);

                try
                {
                    using SqlDataReader reader = command.ExecuteReader();
                    bool result = reader.HasRows;
                    reader.Close();
                    return result;
                }
                catch (SqlException ex)
                {
                    if (SqlServiceWrapper.Run(ex.Number)
                        || EnsureTableCreated(table, ex.Number))
                    {
                        continue;
                    }
                    switch (ex.Number)
                    {
                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private void DeleteGeneric(GenericTable table, ulong guild)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            Reopen();

            while (true)
            {
                SqlCommand command = table.GetDeleteQuery(_connection, guild);

                try
                {
                    int count = command.ExecuteNonQuery();
                    return;
                }
                catch (SqlException ex)
                {
                    if (SqlServiceWrapper.Run(ex.Number)
                        || EnsureTableCreated(table, ex.Number))
                    {
                        continue;
                    }
                    switch (ex.Number)
                    {
                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private void InsertGeneric(GenericTable table, ulong guild, int type, string id, string hyper)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            Reopen();

            while (true)
            {
                SqlCommand command = table.GetInsertQuery(_connection, guild, type, id, hyper);

                try
                {
                    int count = command.ExecuteNonQuery();
                    return;
                }
                catch (SqlException ex)
                {
                    if (SqlServiceWrapper.Run(ex.Number)
                        || EnsureTableCreated(table, ex.Number))
                    {
                        continue;
                    }
                    switch (ex.Number)
                    {
                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private bool EnsureTableCreated(GenericTable table, int error)
        {
            // Invalid object name
            if (error == 208)
            {
                Server server = new(new ServerConnection(_connection));
                _ = server.ConnectionContext.ExecuteNonQuery(table.GetScript());
                return true;
            }
            return false;
        }
    }
}
