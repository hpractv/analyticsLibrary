using analyticsLibrary.dbObjects;
using analyticsLibrary.library;
using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;


namespace analyticsLibrary.oracle
{
    public class oracleDb
    {
        private login _login = null;
        private login login
        {
            get
            {
                return dbConnection.getLoginOracle(ref _login, string.Empty, _server, _service, _port);
            }
            set { _login = value; }
        }

        private string _server;
        public string server { get { return _server; } }
        private string _service;
        public string service { get { return _service; } }
        private int _port;
        public int port { get { return _port; } }
        
        public oracleDb(string server, string service, int port)
            : this()
        {
            _server  = server;
            _service = service;
            _port    = port;
        }

        public oracleDb() { }

        public void primeLogin()
        {
            dbConnection.primeLogin(login, dbConnection.loginType.tns);
        }
        public void primeLogin(string userId, string password)
        {
            login = dbConnection.primeLogin(server, service, userId, password, port);
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
            var connStr = string.Format(@"Data Source={0};User id={1};Password=""{2}"";"
		        , string.Format("(DESCRIPTION= (ADDRESS= (PROTOCOL=TCP) (HOST={0}) (PORT={1}) ) (CONNECT_DATA= (SERVER=dedicated) (SERVICE_NAME={2}) (UR=A) ) )"
                    , login.server, login.port, login.serviceName)
                , login.userId
                , login.password);
            
            var conn = new OracleConnection(connStr);
            conn.Open();

            //trim the sql, becuase oracle can't handle the extra carriage feeds line returns
            var oda = new OracleDataAdapter(sql.Trim(), conn);
            oda.SelectCommand.CommandTimeout = 0;

            var results = new DataTable();
            oda.Fill(results);
            conn.Close();

            return results.Rows.Cast<DataRow>();
        }

        public IEnumerable<table> tablesByName(string schema, string tableName)
        {
            return tablesByName(schema, tableName, true);
        }
        public IEnumerable<table> tablesByName(string schema, string tableName, bool normalizeCase)
        {

            var tables = this.execute(
                string.Format(@"
select owner, table_name
from  all_tables
where owner like '%{0}%' and
    table_name like '%{1}%'
"               , normalizeCase ? schema.ToUpper() : schema
                , normalizeCase ? tableName.ToUpper() : tableName ))
                .Select(c => new table()
                {
                    schema = c["owner"].ToString(),
                    name = c["table_name"].ToString(),
                });

            return tables;
        }
        public IEnumerable<column> columnsByName(string schema, string columnName)
        {
            return columnsByName(schema, columnName, true);
        }
        public IEnumerable<column> columnsByName(string schema, string columnName, bool normalizeCase)
        {
            var columns = this.execute(string.Format(@"
select owner, table_name, column_name, data_type, data_length, nullable
from all_tab_cols
where owner like '%{0}%' and
  column_name like '%{1}%'
"               , normalizeCase ? schema.ToUpper() : schema
                , normalizeCase ? columnName.ToUpper() : columnName))
            .Select(c => new
            {
                table_name = c["table_name"].ToString(),
                name = c["column_name"].ToString(),
                dataType = sqlDb.dataTypeFromString(c["data_type"].ToString()),
                length = c.IsNull("data_length") ? (decimal?)null : (decimal)c["data_length"],
                nullable = c["nullable"].ToString().ToUpper() == "YES" ? true : false,
            });

            var tables = columns.GroupBy(c => c.table_name)
            .Select(c => new table()
            {
                name = c.Key,
            }).ToArray();

            foreach (var t in tables)
            {
                var tc =  columns.Where(c => c.table_name == t.name)
                .Select(c => new column()
                {
                    parentTable = t.name,
                    name = c.name,
                    dataType = c.dataType,
                    length = (int?)c.length,
                    nullable = c.nullable,
                }).ToArray();
                t.columns = tc;
            }

            return tables.SelectMany(t => t.columns);
        }
        public IEnumerable<column> columnsByTable(string schema, string tableName)
        {
            return columnsByTable(schema, tableName, true);

        }
        public IEnumerable<column> columnsByTable(string schema, string tableName, bool normalizeCase)
        {
            var table = new table();

            var columns = this.execute(string.Format(@"
select owner, table_name, column_name, data_type, data_length, nullable
from all_tab_cols
where owner like '%{0}%' and
  table_name = '{1}'
"
                , normalizeCase ? schema.ToUpper() : schema
                , normalizeCase ? tableName.ToUpper() : tableName))
                .Select(c => new
                    {
                        table_name = c["table_name"].ToString(),
                        name = c["column_name"].ToString(),
                        dataType = sqlDb.dataTypeFromString(c["data_type"].ToString()),
                        length = c.IsNull("data_length") ? (decimal?)null : (decimal)c["data_length"],
                        nullable = c["nullable"].ToString().ToUpper() == "YES" ? true : false,
                    });

            table.name = columns.Count() > 0 ? columns.First().table_name : tableName;

            table.columns = columns
                .Select(c => new column()
                {
                    parentTable = table.name,
                    name = c.name,
                    dataType = c.dataType,
                    length = (int?)c.length,
                    nullable = c.nullable,
                });

            return table.columns;
        }
    }
}
