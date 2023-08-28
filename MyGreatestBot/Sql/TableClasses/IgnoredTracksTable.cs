namespace MyGreatestBot.Sql.TableClasses
{
    internal sealed class IgnoredTracksTable : GenericIgnoredTable
    {
        internal IgnoredTracksTable(string database) : base("IgnoredTracks", database)
        {

        }
    }
}
