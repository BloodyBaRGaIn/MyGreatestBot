using DicordNET.ApiClasses;
using Microsoft.Data.SqlClient;

namespace DicordNET.DB
{
    internal static class DataBaseManager
    {
        private const string DB_PATH = "DB\\Database1.mdf";
        private static readonly string CONNECTION_STRING = $"Data Source=(LocalDB)\\MSSQLLocalDB;AttachDbFilename={Directory.GetCurrentDirectory()}\\{DB_PATH};Integrated Security=True";
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
                @"INSERT INTO IgnoredTracksTable (Type, TrackId, Hyper) VALUES (@type, @id, @hyper)", Connection);
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

            if (cnt != 1)
            {
                throw new InvalidOperationException("Cannot add track to ignored");
            }
        }

        internal static bool IsIgnored(ITrackInfo track)
        {
            SqlCommand command = new(@"SELECT Type, TrackId FROM IgnoredTracksTable WHERE Type=@type AND TrackId=@id", Connection);
            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", (int)track.TrackType);
            command.Parameters.AddWithValue("@id", track.Id);

            try
            {
                return command.ExecuteReader().HasRows;
            }
            catch
            {
                return false;
            }
        }
    }
}
