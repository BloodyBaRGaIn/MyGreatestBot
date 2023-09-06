using Microsoft.Data.SqlClient;
using System;

namespace MyGreatestBot.ApiClasses.Services.Sql.TableClasses
{
    internal class BaseTableProvider
    {
        internal string Database { get; }
        internal string Name { get; }

        internal virtual string GetScript()
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetSelectQuery(SqlConnection? connection, ulong guild)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetDeleteQuery(SqlConnection? connection, ulong guild)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetSelectWhereQuery(SqlConnection? connection, params object[] @params)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetInsertQuery(SqlConnection? connection, params object[] @params)
        {
            throw new NotImplementedException();
        }

        internal BaseTableProvider(string name, string database)
        {
            Database = database;
            Name = name;
        }
    }
}
