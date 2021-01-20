using analyticsLibrary.dbObjects;
using analyticsLibrary.excelLibrary;
using analyticsLibrary.library;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Odbc;
using System.Linq;
using System.Text.RegularExpressions;

namespace analyticsLibrary.hadoop
{
    public class hadoopDb
    {
        private login _login = null;
        private login login
        {
            get
            {
                return dbConnection.getLoginDsn(ref _login, string.Empty, _dsn);
            }
            set { _login = value; }
        }
        private string _dsn = string.Empty;
        public string schema
        {
            get;
            private set;
        }

        public hadoopDb(string dsn) :
            this(dsn, null)
        { }
        public hadoopDb(string dsn, string schema)
        {
            _dsn = dsn;
            this.schema = schema ?? "default";
        }

        public void primeLogin()
        {
            dbConnection.primeLogin(login, dbConnection.loginType.dsn);
        }
        public void primeLogin(string dsn, string userId, string password)
        {
            login = dbConnection.primeLogin(dsn, userId, password);
        }

        public IEnumerable<DataRow> execute(string sql, Action<IEnumerable<DataRow>> process)
        {
            var data = execute(sql);
            if (process == null)
            {
                throw new ApplicationException("Process method must be specified.");
            }
            else
            {
                process(data);
            }
            return data;
        }
        public IEnumerable<t> execute<t>(string sql, Func<IEnumerable<DataRow>, IEnumerable<t>> format)
        {
            return execute(sql, format, null);
        }
        private IEnumerable<t> execute<t>(string sql, Func<IEnumerable<DataRow>, IEnumerable<t>> format, Action<IEnumerable<DataRow>> process = null)
        {
            var data = execute(sql);
            if (process != null) process(data);
            if (format == null) throw new ApplicationException("Format method must be specified.");

            return format(data);
        }

        public IEnumerable<DataRow> execute(string sql)
        {
            var connection = string.Format("dsn={0};uid={1};pwd={2};schema={3}", login.dsn, login.userId, login.password, this.schema);

            var conn = new System.Data.Odbc.OdbcConnection();
            conn.ConnectionString = connection;
            conn.ConnectionTimeout = 0;
            conn.Open();

            /* insert into medispan.testid values (1, cast('one' as varchar(100))), (2, cast('two' as varchar(100))), (3, cast('three' as varchar(100))); */
            var oda = new OdbcDataAdapter(sql, conn);
            oda.SelectCommand.CommandTimeout = 0;

            var results = new DataTable();
            oda.Fill(results);
            conn.Close();

            return results.Rows.Cast<DataRow>();
        }

        public IEnumerable<table> tablesByName(string tableName = null)
        {
            return this.execute("SHOW TABLES")
                .Select(r => new table()
                {
                    schema = this.schema,
                    name = r.containsColumn("tab_name") ? r.Field<string>("tab_name") : r.Field<string>("name"),
                })
                .Where(t =>
                    string.IsNullOrWhiteSpace(tableName) ||
                    (!string.IsNullOrWhiteSpace(tableName) && t.name.ToLower().Contains(tableName)));
        }
        public IEnumerable<column> columnsByTable(string tableName)
        {
            var typeRegEx = @"(?<type>[A-z\d_]+>?)(\((?<length>[\d]+?)\))*";
            return execute($"describe {tableName}")
                .Select(c =>
                {
                    var matches = Regex.Match(
                        c.containsColumn("data_type") ? c.Field<string>("data_type") : c.Field<string>("type"),
                        typeRegEx, RegexOptions.IgnoreCase);
                    return new column()
                    {
                        parentTable = tableName,
                        name = c.containsColumn("col_name") ? c.Field<string>("col_name") : c.Field<string>("name"),
                        dataType = sqlDb.dataTypeFromString(matches.Groups["type"].Value),
                        length = string.IsNullOrWhiteSpace(matches.Groups["length"].Value) ? (int?)null : int.Parse(matches.Groups["length"].Value),
                    };
                });
        }
    }

    public static class hadoopExtensions {
        public static string toHadoopString(this DateTime value) =>
        $@"from_unixtime(unix_timestamp('{value.ToString("yyyy-MM-dd;HH:mm:ss")}', 'yyyy-MM-dd;HH:mm:ss'))";
    }
}
