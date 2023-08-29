namespace MyGreatestBot.ApiClasses.Services.Sql.TableClasses
{
    internal sealed class IgnoredArtistsTable : GenericIgnoredTable
    {
        internal IgnoredArtistsTable(string database) : base("IgnoredArtists", database)
        {

        }
    }
}
