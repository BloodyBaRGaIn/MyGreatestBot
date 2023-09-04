﻿using Microsoft.Data.SqlClient;
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

        internal virtual SqlCommand GetSelectQuery(SqlConnection? connection = null)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetDeleteQuery(SqlConnection? connection = null)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetSelectWhereQuery(SqlConnection? connection = null, params object[] @params)
        {
            throw new NotImplementedException();
        }

        internal virtual SqlCommand GetInsertQuery(SqlConnection? connection = null, params object[] @params)
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
