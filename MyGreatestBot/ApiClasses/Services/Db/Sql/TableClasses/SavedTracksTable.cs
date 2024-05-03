namespace MyGreatestBot.ApiClasses.Services.Db.Sql.TableClasses
{
    internal class SavedTracksTable : GenericTable
    {
        internal SavedTracksTable(string database) : base(DbCollectionNames.SavedTracksCollectionName, database)
        {

        }

        internal override string GetScript()
        {
            string createTableString = $"""
                USE [{Database}]
                GO

                SET ANSI_NULLS ON
                GO

                SET QUOTED_IDENTIFIER ON
                GO

                CREATE TABLE [dbo].[{Name}](
                	[Counter] [int] IDENTITY(1,1) NOT NULL,
                	[Guild] [decimal](38, 0) NOT NULL,
                	[Type] [int] NOT NULL,
                	[ID] [char](64) NOT NULL,
                	[Hyper] [char](256) NOT NULL,
                 CONSTRAINT [PK_{Name}] PRIMARY KEY CLUSTERED 
                (
                    [Counter] ASC,
                	[Guild] ASC,
                	[Type] ASC,
                	[ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = ON, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
                GO
                """
            ;

            return createTableString;
        }
    }
}
