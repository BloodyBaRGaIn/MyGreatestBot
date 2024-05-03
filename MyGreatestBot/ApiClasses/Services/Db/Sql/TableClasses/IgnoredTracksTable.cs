namespace MyGreatestBot.ApiClasses.Services.Db.Sql.TableClasses
{
    internal sealed class IgnoredTracksTable : GenericTable
    {
        internal IgnoredTracksTable(string database) : base(DbCollectionNames.IgnoredTracksCollectionName, database)
        {

        }
    }
}
