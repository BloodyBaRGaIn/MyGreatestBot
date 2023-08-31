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

namespace MyGreatestBot.ApiClasses.Services.Sql
{
    [SupportedOSPlatform("windows")]
    internal static class SqlServerWrapper
    {
        private static readonly string ServerString;
        private static readonly string ConnectionString;

        private static readonly IgnoredTracksTable IgnoredTracks;
        private static readonly IgnoredArtistsTable IgnoredArtists;

        private static SqlDatabaseConfigJSON config;

        private static readonly DatabaseScriptProvider scriptProvider;

        private static SqlConnection? _connection;

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
            }
            catch (Exception ex)
            {
                throw new SqlApiException("Initialization failed", ex);
            }
        }

        internal static void Open()
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

        internal static void Close()
        {
            _connection?.Close();
        }

        internal static bool IsTrackIgnored(ITrackInfo track)
        {
            return IsTrackIgnored((int)track.TrackType, track.Id);
        }

        private static bool IsTrackIgnored(int type, string id)
        {
            if (_connection is null)
            {
                return false;
            }

            SqlCommand command = IgnoredTracks.GetSelectQuery(_connection, type, id);

            while (true)
            {
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
                            _ = server.ConnectionContext.ExecuteNonQuery(IgnoredTracks.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        internal static bool IsAnyArtistIgnored(ITrackInfo track)
        {
            return IsAnyArtistIgnored((int)track.TrackType, track.ArtistArr.Select(a => a.InnerId));
        }

        private static bool IsAnyArtistIgnored(int type, IEnumerable<string> ids)
        {
            if (_connection is null || ids is null || !ids.Any())
            {
                return false;
            }

            try
            {
                return ids.Any(id => IsArtistIgnored(type, id));
            }
            catch
            {
                throw;
            }
        }

        private static bool IsArtistIgnored(int type, string id)
        {
            if (_connection is null || string.IsNullOrWhiteSpace(id))
            {
                return false;
            }

            while (true)
            {
                SqlCommand command = IgnoredArtists.GetSelectQuery(_connection, type, id);

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
                            _ = server.ConnectionContext.ExecuteNonQuery(IgnoredArtists.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        internal static void AddIgnoredTrack(int type, string id, string hyper)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            while (true)
            {
                SqlCommand command = IgnoredTracks.GetInsertQuery(_connection, type, id, hyper);

                try
                {
                    int count = command.ExecuteNonQuery();
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
                            _ = server.ConnectionContext.ExecuteNonQuery(IgnoredTracks.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        internal static void AddIgnoredTrack(ITrackInfo track)
        {
            AddIgnoredTrack((int)track.TrackType, track.Id, track.TrackName.ToString());
        }

        private static void AddIgnoredArtist(int type, string id, string hyper)
        {
            if (_connection is null)
            {
                throw new InvalidOperationException("DB connection not initialized");
            }

            while (true)
            {
                SqlCommand command = IgnoredArtists.GetInsertQuery(_connection, type, id, hyper);

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
                            _ = server.ConnectionContext.ExecuteNonQuery(IgnoredArtists.GetScript());
                            break;

                        default:
                            Console.WriteLine(ex.Number);
                            throw;
                    }
                }
            }
        }

        internal static void AddIgnoredArtist(ITrackInfo track, int index)
        {
            HyperLink artist = track.ArtistArr[index];
            AddIgnoredArtist((int)track.TrackType, artist.InnerId, artist.ToString());
        }
    }
}
