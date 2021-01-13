using analyticsLibrary.library;
using SimpleImpersonation;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading;

namespace analyticsLibrary.dbObjects
{
    public class sqlDb : IDisposable
    {
        private login _sqlLogin = null;

        private login sqlLogin
        {
            get { return dbConnection.getLogin(ref _sqlLogin, "SQL", _server, _domain); }
            set { _sqlLogin = value; }
        }

        private string _server { get; set; }
        private string _domain { get; set; }

        public sqlDb(string server) : this(server, string.Empty)
        {
        }

        public sqlDb(string server, string domain) : this()
        {
            _server = server;
            _domain = domain;
        }

        public sqlDb()
        {
        }

        private SqlConnection _connection = null;
        private string _integratedConnectionString = @"Data Source={0};Initial Catalog={1};Integrated Security=true;";
        private string _sqlConnectionString = @"Data Source={0};Initial Catalog={1};user id={2};password={3};";

        public void primeLogin()
        {
            dbConnection.primeLogin(sqlLogin, dbConnection.loginType.server);
        }

        public void primeLogin(string server, string domain, string userId, string password)
        {
            sqlLogin = dbConnection.primeLogin(server, domain, userId, password);
        }

        private void noImpersonate(Action runMethod) => runMethod();

        private void withImpersonate(Action runMethod)
        {
            using (Impersonation.LogonUser(sqlLogin.domain, sqlLogin.userId, sqlLogin.password, LogonType.NewCredentials))
                runMethod();
        }

        public void executeStatment(string db, string sql)
        {
            _dbExecute(db, command =>
            {
                command.CommandText = sql;
                command.ExecuteNonQuery();
            });
        }

        public IEnumerable<DataRow> query(string db, string sql)
        {
            var data = new DataTable();
            _dbExecute(db, command =>
            {
                var adapter = new SqlDataAdapter(command);
                command.CommandText = sql;
                adapter.SelectCommand.CommandTimeout = 0;
                adapter.Fill(data);
                adapter.Dispose();
            });

            return data.Rows.Cast<DataRow>();
        }

        public IEnumerable<t> query<t>(string db, string sql, Func<IEnumerable<DataRow>, IEnumerable<t>> formatData)
        {
            var results = this.query(db, sql);
            if (formatData == null) throw new ApplicationException("Format function must be specified.");
            return formatData(results);
        }

        public IEnumerable<DataRow> query(string db, string sql, Action<IEnumerable<DataRow>> processData)
        {
            var data = this.query(db, sql);
            if (processData == null) throw new ApplicationException("Process function must be specified.");
            processData(data);
            return data;
        }

        public IEnumerable<t> query<t>(string db, string sql, Action<IEnumerable<DataRow>> processData, Func<IEnumerable<DataRow>, IEnumerable<t>> formatData)
        {
            var results = this.query(db, sql);

            if (processData != null) throw new ApplicationException("Process function must be specified.");
            processData(results);

            if (formatData == null) throw new ApplicationException("Format function must be specified");

            return formatData(results);
        }

        private void _dbExecute(string db, Action<SqlCommand> sqlAction)
        {
            var onDomain = !string.IsNullOrWhiteSpace(sqlLogin.domain);
            var connectionString = onDomain ? string.Format(_integratedConnectionString, sqlLogin.server, db) : string.Format(_sqlConnectionString, sqlLogin.server, db, sqlLogin.userId, sqlLogin.password);
            var tryCount = 0;
            var tryMax = 20;
            var impersonateWrapper = onDomain ? (Action<Action>)withImpersonate : (Action<Action>)noImpersonate;

            IMPERSONATE:

            try
            {
                impersonateWrapper(() =>
                {
                    if (_connection == null)
                    {
                        _connection = new SqlConnection(connectionString);
                        _connection.Open();
                    }

                    lock (_connection)
                    {
                        var command = _connection.CreateCommand();
                        command.CommandTimeout = 0;
                        sqlAction(command);
                        command.Dispose();
                    }
                });
            }
            catch (Exception exc)
            {
                var message = exc.Message.ToString().ToLower();
                //try the query 20 times
                if ((message.Contains("login") || message.Contains("logon")) && tryCount++ < tryMax)
                {
                    _connection.Dispose();
                    _connection = null;
                    Thread.Sleep(1000); //ait a second
                    goto IMPERSONATE;   //then try to login again
                }
                else
                {
                    throw exc;
                }
            }
        }

        public IEnumerable<table> tablesByName(string db, string tableName)
        {
            var tables = this.query(db,
                string.Format(
                    @"select t.table_schema, t.table_name
			        from information_schema.tables t
			        where t.table_name like '%{0}%'
			        order by t.table_schema, t.table_name",
                    tableName))
            .Select(c => new table()
            {
                schema = c["table_schema"].ToString(),
                name = c["table_name"].ToString(),
            });

            return tables;
        }

        public IEnumerable<column> columnsByName(string db, string columnName)
        {
            var columns = this.query(db,
                string.Format(
                    @"select c.table_name, c.column_name, c.data_type, c.character_maximum_length, c.is_nullable
                    from information_schema.columns c
                    where c.column_name like '%{0}%'
                    order by c.table_name",
                columnName))
            .Select(c => new
            {
                table_name = c["table_name"].ToString(),
                name = c["column_name"].ToString(),
                dataType = dataTypeFromString(c["data_type"].ToString()),
                length = c.IsNull("character_maximum_length") ? (int?)null : (int)c["character_maximum_length"],
                nullable = c["is_nullable"].ToString() == "YES" ? true : false,
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

        public IEnumerable<column> columnsByTable(string db, string tableName)
        {
            var table = new table();

            var columns = this.query(db,
                string.Format(
                    @"select c.table_name, c.column_name, c.data_type, c.character_maximum_length, c.is_nullable
			        from information_schema.columns c
			        where c.table_name like '%{0}%'
			        order by c.column_name",
                    tableName))
            .Select(c => new
            {
                table_name = c["table_name"].ToString(),
                name = c["column_name"].ToString(),
                dataType = dataTypeFromString(c["data_type"].ToString()),
                length = c.IsNull("character_maximum_length") ? (int?)null : (int)c["character_maximum_length"],
                nullable = c["is_nullable"].ToString() == "YES" ? true : false,
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

        public static dataTypeEnum dataTypeFromString(string type)
        {
            var returnType = dataTypeEnum.unknown;
            switch (type.ToLower())
            {
                case "bigint":
                    returnType = dataTypeEnum.bigintType;
                    break;

                case "bit":
                    returnType = dataTypeEnum.bitType;
                    break;

                case "char":
                    returnType = dataTypeEnum.charType;
                    break;

                case "date":
                    returnType = dataTypeEnum.dateType;
                    break;

                case "datetime":
                    returnType = dataTypeEnum.datetimeType;
                    break;

                case "decimal":
                    returnType = dataTypeEnum.decimalType;
                    break;

                case "float":
                    returnType = dataTypeEnum.floatType;
                    break;

                case "int":
                    returnType = dataTypeEnum.intType;
                    break;

                case "money":
                    returnType = dataTypeEnum.moneyType;
                    break;

                case "long":
                case "number":
                case "numeric":
                    returnType = dataTypeEnum.numericType;
                    break;

                case "nvarchar":
                case "nvarchar2":
                    returnType = dataTypeEnum.nvarcharType;
                    break;

                case "smalldatetime":
                    returnType = dataTypeEnum.smalldatetimeType;
                    break;

                case "timestamp":
                case "timestamp(0)":
                case "timestamp(3)":
                case "timestamp(6)":
                case "timestamp(9)":
                    returnType = dataTypeEnum.timestampType;
                    break;

                case "tinyint":
                    returnType = dataTypeEnum.tinyintType;
                    break;

                case "varbinary":
                    returnType = dataTypeEnum.varbinaryType;
                    break;

                case "varchar":
                case "varchar2":
                    returnType = dataTypeEnum.varcharType;
                    break;
            }
            return returnType;
        }

        public void Dispose()
        {
            if (_connection.State == ConnectionState.Open) _connection.Close();
            _connection.Dispose();
            GC.Collect();
        }
    }
}
