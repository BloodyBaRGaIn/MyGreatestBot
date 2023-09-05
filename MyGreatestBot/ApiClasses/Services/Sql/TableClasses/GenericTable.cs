using Microsoft.Data.SqlClient;
using System;

namespace MyGreatestBot.ApiClasses.Services.Sql.TableClasses
{
    internal class GenericTable : BaseTableProvider
    {
        internal GenericTable(string name, string database) : base(name, database)
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
                	[Guild] ASC,
                	[Type] ASC,
                	[ID] ASC
                )WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
                ) ON [PRIMARY]
                GO
                """
            ;

            return createTableString;
        }

        internal override SqlCommand GetSelectQuery(SqlConnection? connection, ulong guild)
        {
            SqlCommand command = new($"SELECT Type, ID FROM {Name} WHERE Guild=@guild ORDER BY Counter ASC");

            command.Parameters.Clear();
            command.Parameters.Add("@guild", System.Data.SqlDbType.Decimal, 38).Value = Convert.ToDecimal(guild);

            command.Connection = connection;

            return command;
        }

        internal override SqlCommand GetDeleteQuery(SqlConnection? connection, ulong guild)
        {
            SqlCommand command = new($"DELETE FROM {Name} WHERE Guild=@guild");

            command.Parameters.Clear();
            command.Parameters.Add("@guild", System.Data.SqlDbType.Decimal, 38).Value = Convert.ToDecimal(guild);

            command.Connection = connection;

            return command;
        }

        /// <summary>
        /// Select where
        /// </summary>
        /// <param name="connection"></param>
        /// <param name="params">Type, ID</param>
        /// <returns>Connamd to execute</returns>
        internal override SqlCommand GetSelectWhereQuery(SqlConnection? connection = null, params object[] @params)
        {
            if (@params.Length != 3)
            {
                throw new ArgumentException("Wrong params count");
            }

            if (!ulong.TryParse(@params[0].ToString(), out ulong guild))
            {
                throw new InvalidOperationException($"Cannot cast {@params[0]} to {typeof(ulong).Name}");
            }

            if (!int.TryParse(@params[1].ToString(), out int type))
            {
                throw new InvalidOperationException($"Cannot cast {@params[1]} to {typeof(int).Name}");
            }

            if (@params[2] is not string id)
            {
                throw new InvalidOperationException($"Cannot cast {@params[2]} to {typeof(string).Name}");
            }

            SqlCommand command = new($"SELECT Type, ID FROM {Name} WHERE Guild=@guild AND Type=@type AND ID=@id");

            command.Parameters.Clear();
            command.Parameters.Add("@guild", System.Data.SqlDbType.Decimal, 38).Value = Convert.ToDecimal(guild);
            _ = command.Parameters.AddWithValue("@type", type);
            _ = command.Parameters.AddWithValue("@id", id);

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
            if (@params.Length != 4)
            {
                throw new ArgumentException("Wrong params count");
            }

            if (!ulong.TryParse(@params[0].ToString(), out ulong guild))
            {
                throw new InvalidOperationException($"Cannot cast {@params[0]} to {typeof(ulong).Name}");
            }

            if (!int.TryParse(@params[1].ToString(), out int type))
            {
                throw new InvalidOperationException($"Cannot cast {@params[1]} to {typeof(int).Name}");
            }

            if (@params[2] is not string id)
            {
                throw new InvalidOperationException($"Cannot cast {@params[2]} to {typeof(string).Name}");
            }

            if (@params[3] is not string hyper)
            {
                throw new InvalidOperationException($"Cannot cast {@params[3]} to {typeof(string).Name}");
            }

            SqlCommand command = new($"INSERT INTO {Name} (Guild, Type, ID, Hyper) VALUES (@guild, @type, @id, @hyper)");

            command.Parameters.Clear();
            command.Parameters.Add("@guild", System.Data.SqlDbType.Decimal, 38).Value = Convert.ToDecimal(guild);
            _ = command.Parameters.AddWithValue("@type", type);
            _ = command.Parameters.AddWithValue("@id", id);
            _ = command.Parameters.AddWithValue("@hyper", hyper);

            command.Connection = connection;

            return command;
        }
    }
}
