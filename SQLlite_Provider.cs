using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;

namespace Bot
{
    internal class SQLiteProvider
    {

        public SqliteConnection Connection { get; }
        public SQLiteProvider()
        {
            Connection = new SqliteConnection("Data Source=cve_db.db");

        }
    }
}
