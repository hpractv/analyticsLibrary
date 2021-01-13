using analyticsLibrary.library;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Linq;

namespace analyticsLibrary.access
{
    public class access
    {
        public string filePath { get; private set; }
        public access(string filePath) => this.filePath = filePath;


        private OleDbConnection _connection;
        private OleDbConnection connection
        {
            get
            {
                _connection = _connection ?? new OleDbConnection() { ConnectionString = $@"Provider=Microsoft.ACE.OLEDB.12.0;Data source = '{ this.filePath }';" };
                return _connection;
            }
        }

        public IEnumerable<DataRow> execute(string sql)
        {
            connection.Open();

            var oda = new OleDbDataAdapter(sql, connection);
            oda.SelectCommand.CommandTimeout = 0;

            var results = new DataTable();
            oda.Fill(results);
            connection.Close();

            return results.Rows.Cast<DataRow>();
        }


        public IEnumerable<table> tablesByName(string tableName = null)
        {
            var results = (IEnumerable<DataRow>)null;

            try
            {
                connection.Open();
                results = connection.GetSchema("Tables")
                    .Rows.Cast<DataRow>()
                    .ToArray();
            }
            finally
            {

                connection.Close();
            }

            return results?
                .Select(r => new table()
                {
                    schema = r.Field<string>("TABLE_SCHEMA"),
                    name = r.Field<string>("TABLE_NAME"),
                })
                .Where(r => tableName == null || (tableName != null && r.name.ToLower().Contains(tableName.ToLower())));
        }

        private IEnumerable<column> columns()
        {
            var results = (IEnumerable<DataRow>)null;

            try
            {
                connection.Open();
                results = connection.GetSchema("Columns")
                    .Rows.Cast<DataRow>()
                    .ToArray();
            }
            finally
            {
                connection.Close();
            }

            return results?
                .Select(c => new column()
                {
                    parentTable = c["table_name"].ToString(),
                    name = c["column_name"].ToString(),
                    dataType = dataTypeFromInt(c.Field<int?>("data_type")),
                    length = c.IsNull("character_maximum_length") ? (int?)null : int.Parse(c["character_maximum_length"].ToString()),
                    nullable = c["is_nullable"].ToString().ToUpper() == "TRUE" ? true : false,
                });
        }
        public IEnumerable<column> columnsByTable(string tableName = null)
        {

            return columns()
                ?.Where(r => tableName == null || (tableName != null && r.parentTable.ToLower().Contains(tableName.ToLower())));

        }
        public IEnumerable<column> columnsByName(string columnName = null)
        {

            return columns()
                ?.Where(r => columnName == null || (columnName != null && r.name.ToLower().Contains(columnName.ToLower())));
        }


        public static dataTypeEnum dataTypeFromInt(int? type)
        {
            var returnType = dataTypeEnum.unknown;
            switch (type)
            {
                case 20:
                    returnType = dataTypeEnum.bigintType;
                    break;
                case 11:
                    returnType = dataTypeEnum.bitType;
                    break;
                case 129:
                case 130:
                    returnType = dataTypeEnum.charType;
                    break;
                case 7:
                case 64:
                case 133:
                case 134:
                case 135:
                    returnType = dataTypeEnum.datetimeType;
                    break;
                case 131:
                    returnType = dataTypeEnum.decimalType;
                    break;
                case 4:
                case 5:
                    returnType = dataTypeEnum.floatType;
                    break;
                case 3:
                case 19:
                    returnType = dataTypeEnum.intType;
                    break;
                case 6:
                    returnType = dataTypeEnum.moneyType;
                    break;
                case 2:
                case 16:
                case 17:
                case 18:
                    returnType = dataTypeEnum.tinyintType;
                    break;
                case 200:
                case 201:
                    returnType = dataTypeEnum.varcharType;
                    break;
                case null:
                default:
                    returnType = dataTypeEnum.unknown;
                    break;
            }
            return returnType;
        }

    }
}
