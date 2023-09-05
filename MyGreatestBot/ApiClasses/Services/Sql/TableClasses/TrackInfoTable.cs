namespace MyGreatestBot.ApiClasses.Services.Sql.TableClasses
{
    internal class TrackInfoTable : GenericTable
    {
        internal TrackInfoTable(string database) : base("SavedTracks", database)
        {

        }
    }
}
