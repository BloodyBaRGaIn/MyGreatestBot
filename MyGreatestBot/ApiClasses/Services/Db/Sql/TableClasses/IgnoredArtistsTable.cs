namespace MyGreatestBot.ApiClasses.Services.Db.Sql.TableClasses
{
    internal sealed class IgnoredArtistsTable : GenericTable
    {
        internal IgnoredArtistsTable(string database) : base(DbCollectionNames.IgnoredArtistsCollectionName, database)
        {

        }
    }
}
