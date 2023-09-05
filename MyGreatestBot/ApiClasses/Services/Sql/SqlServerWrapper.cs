using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using MyGreatestBot.ApiClasses.ConfigStructs;
using MyGreatestBot.ApiClasses.Exceptions;
using MyGreatestBot.ApiClasses.Services.Sql.TableClasses;
using MyGreatestBot.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.ApiClasses.Services.Sql
{
    [SupportedOSPlatform("windows")]
    public static class SqlServerWrapper
    {
        private static readonly string ServerString;
        private static readonly string ConnectionString;

        private static readonly IgnoredTracksTable IgnoredTracks;
        private static readonly IgnoredArtistsTable IgnoredArtists;
        private static readonly TrackInfoTable TrackInfo;

        private static SqlDatabaseConfigJSON config;

        private static readonly DatabaseScriptProvider scriptProvider;

        private static SqlConnection? _connection;

        private static readonly object _lock = 0;

        static SqlServerWrapper()
        {
            try
            {
                config = ConfigManager.GetSqlDatabaseConfigJSON();

                scriptProvider = new()
                {
                    LocalStoreDirectory = config.LocalDirectory,
                    DatabaseName = config.DatabaseName
                };

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

        public static void Open()
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
                    switch (ex.Number)
                    {
                        case -2:
                        case -1:
                        case 1:
                        case 2:
                            SqlServiceWrapper.Run();
                            break;

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

        public static void Close()
        {
            _connection?.Close();
        }

        internal static bool IsTrackIgnored(ITrackInfo track, ulong guild)
        {
            lock (_lock)
            {
                return IsTrackIgnored(guild, (int)track.TrackType, track.Id);
            }
        }

        internal static bool IsAnyArtistIgnored(ITrackInfo track, ulong guild)
        {
            lock (_lock)
            {
                return IsAnyArtistIgnored(guild, (int)track.TrackType, track.ArtistArr.Select(a => a.InnerId));
            }
        }

        internal static void AddIgnoredTrack(ITrackInfo track, ulong guild)
        {
            lock (_lock)
            {
                AddIgnoredTrack(guild, (int)track.TrackType, track.Id, track.TrackName.ToString());
            }
        }

        internal static void AddIgnoredArtist(ITrackInfo track, int index, ulong guild)
        {
            lock (_lock)
            {
                HyperLink artist = track.ArtistArr[index];
                AddIgnoredArtist(guild, (int)track.TrackType, artist.InnerId, artist.ToString());
            }
        }

        internal static void SaveTracks(IEnumerable<ITrackInfo> tracks, ulong guild)
        {
            foreach (ITrackInfo track in tracks)
            {
                lock (_lock)
                {
                    AddSavedTrack(guild, (int)track.TrackType, track.Id, track.TrackName.ToString());
                }
            }
        }

        internal static List<(ApiIntents, string)> RestoreTracks(ulong guild)
        {
            List<(ApiIntents, string)> items = new();
            lock (_lock)
            {
                try
                {
                    Close();
                    Open();
                }
                catch
                {
                    return items;
                }

                SqlCommand commandSelect = TrackInfo.GetSelectQuery(_connection, guild);
                SqlDataReader? reader = null;
                try
                {
                    reader = commandSelect.ExecuteReader();
                }
                catch (SqlException ex)
                {
                    switch (ex.Number)
                    {
                        case -2:
                        case -1:
                        case 1:
                        case 2:
                            SqlServiceWrapper.Run();
                            break;

                        // Invalid object name
                        case 208:
                            Server server = new(new ServerConnection(_connection));
                            _ = server.ConnectionContext.ExecuteNonQuery(TrackInfo.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
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

        internal static void RemoveTracks(ulong guild)
        {
            lock (_lock)
            {
                DeleteGeneric(TrackInfo, guild);
                try
                {
                    SqlCommand command = new($"DBCC CHECKIDENT ('[{TrackInfo.Name}]', RESEED, 0)", _connection);
                    _ = command.ExecuteNonQuery();
                }
                catch
                {
                    ;
                }
            }
        }

        private static bool IsTrackIgnored(ulong guild, int type, string id)
        {
            return GenericHasRows(IgnoredTracks, guild, type, id);
        }

        private static bool IsAnyArtistIgnored(ulong guild, int type, IEnumerable<string> ids)
        {
            if (_connection is null || ids is null || !ids.Any())
            {
                return false;
            }

            try
            {
                return ids.Any(id => IsArtistIgnored(guild, type, id));
            }
            catch
            {
                throw;
            }
        }

        private static bool IsArtistIgnored(ulong guild, int type, string id)
        {
            return GenericHasRows(IgnoredArtists, guild, type, id);
        }

        private static bool GenericHasRows(GenericTable table, ulong guild, int type, string id)
        {
            if (_connection is null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            Close();
            Open();

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
                    switch (ex.Number)
                    {
                        case -2:
                        case -1:
                        case 1:
                        case 2:
                            SqlServiceWrapper.Run();
                            break;

                        // Invalid object name
                        case 208:
                            Server server = new(new ServerConnection(_connection));
                            _ = server.ConnectionContext.ExecuteNonQuery(table.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private static void DeleteGeneric(GenericTable table, ulong guild)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            Close();
            Open();

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
                    switch (ex.Number)
                    {
                        case -2:
                        case -1:
                        case 1:
                        case 2:
                            SqlServiceWrapper.Run();
                            break;

                        // Invalid object name
                        case 208:
                            Server server = new(new ServerConnection(_connection));
                            _ = server.ConnectionContext.ExecuteNonQuery(table.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private static void AddGenericRecord(GenericTable table, ulong guild, int type, string id, string hyper)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            Close();
            Open();

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
                    switch (ex.Number)
                    {
                        case -2:
                        case -1:
                        case 1:
                        case 2:
                            SqlServiceWrapper.Run();
                            break;

                        // Invalid object name
                        case 208:
                            Server server = new(new ServerConnection(_connection));
                            _ = server.ConnectionContext.ExecuteNonQuery(table.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        private static void AddIgnoredTrack(ulong guild, int type, string id, string hyper)
        {
            AddGenericRecord(IgnoredTracks, guild, type, id, hyper);
        }

        private static void AddIgnoredArtist(ulong guild, int type, string id, string hyper)
        {
            AddGenericRecord(IgnoredArtists, guild, type, id, hyper);
        }

        private static void AddSavedTrack(ulong guild, int type, string id, string hyper)
        {
            AddGenericRecord(TrackInfo, guild, type, id, hyper);
        }
    }
}
