using Microsoft.Data.SqlClient;

namespace DicordNET.Sql.TableClasses
{
    internal class GenericIgnoredTable : BaseTableProvider
    {
        internal GenericIgnoredTable(string name, string database) : base(name, database)
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
                    [Type] [int] NOT NULL,
                    [ID] [char](64) NOT NULL,
                    [Hyper] [char](256) NOT NULL,
                 CONSTRAINT [PK_{Name}] PRIMARY KEY CLUSTERED 
                (
                	[Type] ASC,
                	[ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
                GO 
                """
            ;

            return createTableString;
        }

        /// <summary>
        /// Select where
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="params">Type, ID</param>
        /// <returns>Connamd to execute</returns>
        internal override SqlCommand GetSelectQuery(SqlConnection? connection = null, params object[] @params)
        {
            if (@params.Length != 2)
            {
                throw new ArgumentException("Wrong params count");
            }

            if (!int.TryParse(@params[0].ToString(), out int type))
            {
                throw new InvalidOperationException($"Cannot cast {@params[0]} to {typeof(int).Name}");
            }

            if (@params[1] is not string id)
            {
                throw new InvalidOperationException($"Cannot cast {@params[1]} to {typeof(string).Name}");
            }

            SqlCommand command = new($"SELECT Type, ID FROM {Name} WHERE Type=@type AND ID=@id");

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);

            command.Connection = connection;

            return command;
        }

        /// <summary>
        /// Insert into values
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="params">Type, ID, Hyper</param>
        /// <returns>Connamd to execute</returns>
        internal override SqlCommand GetInsertQuery(SqlConnection? connection = null, params object[] @params)
        {
            if (@params.Length != 3)
            {
                throw new ArgumentException("Wrong params count");
            }

            if (!int.TryParse(@params[0].ToString(), out int type))
            {
                throw new InvalidOperationException($"Cannot cast {@params[0]} to {typeof(int).Name}");
            }

            if (@params[1] is not string id)
            {
                throw new InvalidOperationException($"Cannot cast {@params[1]} to {typeof(string).Name}");
            }

            if (@params[2] is not string hyper)
            {
                throw new InvalidOperationException($"Cannot cast {@params[2]} to {typeof(string).Name}");
            }

            SqlCommand command = new($"INSERT INTO {Name} (Type, ID, Hyper) VALUES (@type, @id, @hyper)");

            command.Parameters.Clear();
            command.Parameters.AddWithValue("@type", type);
            command.Parameters.AddWithValue("@id", id);
            command.Parameters.AddWithValue("@hyper", hyper);

            command.Connection = connection;

            return command;
        }
    }
}
