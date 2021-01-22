using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using analyticsLibrary.library;

namespace analyticsLibrary.dbObjects
{
    public static class dataLibrary
    {
        private static string connectionString = @"Data Source={0};Initial Catalog={1};Integrated Security=True;MultipleActiveResultSets=True;Application Name=ReportLibraryBulkCopy";

        public static DataTable buildBulkCopyTable<t>()
        {
            var table = new DataTable();
            var type = typeof(t);
            var properties = getColumnProperties(type);

            foreach (var property in properties)
            {
                Type propertyType = property.PropertyType;
                if (propertyType.IsGenericType &&
                    propertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    propertyType = Nullable.GetUnderlyingType(propertyType);
                }

                table.Columns.Add(new DataColumn(property.Name, propertyType));
            }
            return table;
        }

        public static object[] addTableRecord<t>(t data)
        {
            var type = typeof(t);
            var properties = getColumnProperties(type);
            return properties.Select(property => GetPropertyValue(property.GetValue(data, null))).ToArray();
        }

        public static PropertyInfo[] getColumnProperties(Type type)
        {
            var properties = type.GetProperties()
                .Where(EventTypeFilter)
                .Where(p => p.PropertyType.AssemblyQualifiedName.ToLower().StartsWith("system"))
                .ToArray();
            return properties;
        }

        public static void pushBulkCopyTable<t>(string server, string dataBase, DataTable table)
        {
            pushBulkCopyTable<t>(server, dataBase, null, table);
        }

        public static void pushBulkCopyTable<t>(string server, string dataBase, string schema, DataTable table)
        {
            var connection = new SqlConnection(string.Format(connectionString, server, dataBase));
            pushBulkCopyTable<t>(connection, schema, table, true);
        }

        public static void pushBulkCopyTable<t>(SqlConnection connection, string schema, DataTable table)
        {
            pushBulkCopyTable<t>(connection, schema, table, false);
        }

        public static void pushBulkCopyTable<t>(SqlConnection connection, string schema, DataTable table, bool disposeAndCloseConnection)
        {
            var type = typeof(t);
            var bulkCopy = new SqlBulkCopy(connection) { DestinationTableName = string.Format("[{0}].[{1}]", (schema ?? "dbo"), type.Name) };

            try
            {
                bulkCopy.BulkCopyTimeout = 0;
                if (connection.State != ConnectionState.Open)
                    connection.Open();
                bulkCopy.WriteToServer(table);
                bulkCopy.Close();
            }
            finally
            {
                if (disposeAndCloseConnection)
                {
                    if (connection.State == ConnectionState.Open)
                        connection.Close();
                    connection.Dispose();
                }
            }
        }

        public static bool EventTypeFilter(System.Reflection.PropertyInfo p)
        {
            var allowedTypes = new string[] { "ValueType", "Object" };

            if (!allowedTypes.hasValue(p.PropertyType.BaseType.Name) || (p.PropertyType.BaseType.Name == "Object" && p.Name == "EntityKey"))
                return false;

            return true;
        }

        public static object GetPropertyValue(object o)
        {
            if (o == null)
                return DBNull.Value;
            return o;
        }
    }
}