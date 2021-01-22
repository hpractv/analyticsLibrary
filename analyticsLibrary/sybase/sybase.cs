using analyticsLibrary.dbObjects;
using analyticsLibrary.library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;


namespace analyticsLibrary.sybase
{
    public class sybaseDb
    {

        public string userId { get; }
        public string password { get; }
        public string dsn { get; }
        public string database { get; }


        public sybaseDb(string userId, string password, string dsn, string database)
        {
            this.userId = userId;
            this.password = password;
            this.dsn = dsn;
            this.database = database;
        }

        public IEnumerable<t> execute<t>(string sql, Action<IEnumerable<DataRow>> processData)
        {
            return execute<t>(sql, processData, (Func<IEnumerable<DataRow>, IEnumerable<t>>)null);
        }
        public IEnumerable<t> execute<t>(string sql, Func<IEnumerable<DataRow>, IEnumerable<t>> formatData)
        {
            return execute<t>(sql, (Action<IEnumerable<DataRow>>)null, formatData);
        }
        public IEnumerable<t> execute<t>(string sql, Action<IEnumerable<DataRow>> process = null, Func<IEnumerable<DataRow>, IEnumerable<t>> format = null)
        {
            var results = execute(sql);
            return execute(results, process, format);
        }
        private IEnumerable<t> execute<t>(IEnumerable<DataRow> data, Action<IEnumerable<DataRow>> process = null, Func<IEnumerable<DataRow>, IEnumerable<t>> format = null)
        {
            if (process != null) process(data);
            if (format == null) throw new ApplicationException("Format function must be specified");
            return format(data);
        }
        public IEnumerable<DataRow> execute(string sql)
        {
            var connection = $"dsn={this.dsn};uid={this.userId};pwd={this.password};";

            var conn = new System.Data.Odbc.OdbcConnection();
            conn.ConnectionString = connection;
            conn.ConnectionTimeout = 0;
            conn.Open();

            var oda = new OdbcDataAdapter(sql, conn);
            oda.SelectCommand.CommandTimeout = 0;

            var results = new DataTable();
            oda.Fill(results);
            conn.Close();

            return results.Rows.Cast<DataRow>();
        }

        public IEnumerable<table> tablesByName(string tableName)
        {
            var tables = this.execute($@"sp_tables '%{tableName}%', 'dbo', '{this.database}', ""'TABLE'"";")
                .Select(c => new table()
                {
                    schema = c["table_owner"].ToString(),
                    name = c["table_name"].ToString(),
                });

            return tables;
        }
        public IEnumerable<column> columnsByName(string columnName)
        {
            var columns = this.execute($@"sp_columns @table_name = null, @column_name = '%{columnName}%'")
            .Select(c => new
            {
                table_name = c["table_name"].ToString(),
                name = c["column_name"].ToString(),
                dataType = sqlDb.dataTypeFromString(c["type_name"].ToString()),
                length = c.IsNull("length") ? (int?)null : (int)c["length"],
                nullable = c["is_nullable"].ToString().ToUpper() == "YES" ? true : false,
            });

            var tables = columns.GroupBy(c => c.table_name)
            .Select(c => new table()
            {
                name = c.Key,
            }).ToArray();

            foreach (var t in tables)
            {
                var tc = columns.Where(c => c.table_name == t.name)
                .Select(c => new column()
                {
                    parentTable = t.name,
                    name = c.name,
                    dataType = c.dataType,
                    length = c.length,
                    nullable = c.nullable,
                }).ToArray();
                t.columns = tc;
            }

            return tables.SelectMany(t => t.columns);
        }
        public IEnumerable<column> columnsByTable(string tableName)
        {
            var table = new table();
            var columns = this
                .execute($@"exec sp_columns '%{tableName}%', 'dbo', '{this.database}';")
                .Select(c => new
                {
                    table_name = c["table_name"].ToString(),
                    name = c["column_name"].ToString(),
                    dataType = sqlDb.dataTypeFromString(c["type_name"].ToString()),
                    length = c.IsNull("length") ? (int?)null : (int)c["length"],
                    nullable = c["is_nullable"].ToString().ToUpper() == "YES" ? true : false,
                });

            table.name = columns.Count() > 0 ? columns.First().table_name : tableName;

            table.columns = columns
                .Select(c => new column()
                {
                    parentTable = table.name,
                    name = c.name,
                    dataType = c.dataType,
                    length = c.length,
                    nullable = c.nullable,
                });

            return table.columns;
        }
    }
}
