using DicordNET.ApiClasses;
using DicordNET.Utils;
using Microsoft.Data.SqlClient;

namespace DicordNET.DB
{
    internal static class DataBaseManager
    {
        private const string DB_PATH = "DB\\BotStorageDB.mdf";
        private static readonly string CONNECTION_STRING = "Data Source=USER-PC\\MSSQLSERVER01;Initial Catalog=master;Integrated Security=True;Persist Security Info=False;Pooling=False;Multiple Active Result Sets=False;Connect Timeout=60;Encrypt=False;Trust Server Certificate=False;Command Timeout=0";//$@"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename={Directory.GetCurrentDirectory()}\{DB_PATH};Integrated Security=True";
        private static SqlConnection? Connection;

        internal static void Open()
        {
            Connection = new(CONNECTION_STRING);
            Connection.Open();
        }

        internal static void Close()
        {
            Connection?.Close();
            Connection?.Dispose();
        }

        internal static void AddIgnoredTrack(ITrackInfo track)
        {
            SqlCommand command = new(
                @"INSERT INTO IgnoredTracksTable " +
                @"(Type, TrackId, Hyper) " +
                @"VALUES (@type, @id, @hyper)", Connection);

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", (int)track.TrackType);
            command.Parameters.AddWithValue("@id", track.Id);
            command.Parameters.AddWithValue("@hyper", track.TrackName.ToString());

            int cnt;
            try
            {
                cnt = command.ExecuteNonQuery();
            }
            catch
            {
                cnt = 0;
            }

            if (cnt < 1)
            {
                throw new InvalidOperationException("Cannot add track to ignored");
            }
        }

        internal static bool IsIgnored(ITrackInfo track)
        {
            SqlCommand command = new(
                @"SELECT Type, TrackId FROM IgnoredTracksTable " +
                @"WHERE Type=@type AND TrackId=@id", Connection);

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", (int)track.TrackType);
            command.Parameters.AddWithValue("@id", track.Id);

            try
            {
                var reader = command.ExecuteReader();
                bool result = reader.HasRows;
                reader.Close();
                return result;
            }
            catch
            {
                return false;
            }
        }

        internal static void AddIgnoredArtist(ITrackInfo track, int artist_index = 0)
        {
            if (track.ArtistArr.Length <= artist_index)
            {
                throw new ArgumentException("Invalid artist index");
            }

            HyperLink artist = track.ArtistArr[artist_index];

            SqlCommand command = new(
                @"INSERT INTO IgnoredArtistsTable " +
                @"(Type, ArtistId, Hyper) " +
                @"VALUES (@type, @id, @hyper)", Connection);

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", (int)track.TrackType);
            command.Parameters.AddWithValue("@id", artist.InnerId);
            command.Parameters.AddWithValue("@hyper", artist.ToString());

            int cnt;
            try
            {
                cnt = command.ExecuteNonQuery();
            }
            catch
            {
                cnt = 0;
            }

            if (cnt < 1)
            {
                throw new InvalidOperationException("Cannot add artist to ignored");
            }
        }

        internal static bool IsArtistIgnored(ITrackInfo track)
        {
            bool found = false;
            foreach (HyperLink artist in track.ArtistArr)
            {
                SqlCommand command = new(
                    @"SELECT Type, ArtistId FROM IgnoredArtistsTable " +
                    @"WHERE Type=@type AND ArtistId=@id", Connection);

                command.Parameters.Clear();
                command.Parameters.AddWithValue("@type", (int)track.TrackType);
                command.Parameters.AddWithValue("@id", artist.InnerId);

                try
                {
                    var reader = command.ExecuteReader();
                    found = reader.HasRows;
                    reader.Close();
                }
                catch { }

                if (found)
                {
                    break;
                }
            }

            return found;
        }
    }
}
