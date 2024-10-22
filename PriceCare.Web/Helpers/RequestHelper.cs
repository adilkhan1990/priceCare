using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace PriceCare.Web.Helpers
{
    public static class RequestHelper
    {
        public static void ExecuteQuery(string connectionString, string commandText, Action<DataRow> mapRow, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    mapRow(row);
                }
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = 1000;
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }

        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, object> parameters, Dictionary<string,List<int>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray";
                }
                
                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.SelectCommand.CommandTimeout = 100;
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, List<int>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray";
                }

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, object> parameters, Dictionary<string, IEnumerable<Tuple<int, int>>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }

                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray2";
                }

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, IEnumerable<Tuple<int, int>>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray2";
                }

                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }

        public static DataTable ExecuteQuery(string connectionString, string commandText)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                var adapter = new SqlDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                return table;
            }
        }

        public static T ExecuteScalarRequest<T>(string connectionString, string commandText)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                return (T)command.ExecuteScalar();
            }
        }

        public static T ExecuteScalarRequest<T>(string connectionString, string commandText, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                return (T)command.ExecuteScalar();
            }
        }

        public static int ExecuteNonQuery(string connectionString, string commandText)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                
                return command.ExecuteNonQuery();
            }
        }

        public static int ExecuteNonQuery(string connectionString, string commandText, Dictionary<string, object> parameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                command.CommandTimeout = 1000;
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Key, parameter.Value);
                }
                return command.ExecuteNonQuery();
            }
        }
        public static int ExecuteNonQuery(string connectionString, string commandText, Dictionary<string, List<int>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray";
                }
                return command.ExecuteNonQuery();
            }
        }
        public static int ExecuteNonQuery(string connectionString, string commandText, Dictionary<string, List<Tuple<int,int,int>>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.IdArray3";
                }
                return command.ExecuteNonQuery();
            }
        }
        public static int ExecuteNonQuery(string connectionString, string commandText, Dictionary<string, List<string>> tableParameters)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = commandText;
                foreach (var parameter in tableParameters.Select(tP => command.Parameters.AddWithValue(tP.Key, CreateDataTable(tP.Value))))
                {
                    parameter.SqlDbType = SqlDbType.Structured;
                    parameter.TypeName = "dbo.PermIdArray";
                }
                return command.ExecuteNonQuery();
            }
        }
        public static void BulkInsert(string connectionString, string tableName, DataTable data)
        {
            using (var bulkCopy = new SqlBulkCopy(connectionString))
            {
                bulkCopy.BulkCopyTimeout = 1000;
                bulkCopy.DestinationTableName = tableName;
                bulkCopy.WriteToServer(data);
            }
        }

        public static double? PreventDBNull(object val)
        {
            if (val is DBNull)
                return null;
            return (double)val;
        }

        public static double? PreventNull(this object val)
        {
            if (val is DBNull)
                return null;
            return (double)val;
        }
        public static int? PreventIntNull(this object val)
        {
            if (val is DBNull)
                return null;
            return (int)val;
        }
        public static string PreventStringNull(this object val)
        {
            if (val is DBNull)
                return null;
            return (string)val;
        }

        private static DataTable CreateDataTable(IEnumerable<int> ids)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            foreach (int id in ids)
            {
                table.Rows.Add(id);
            }
            return table;
        }
        private static DataTable CreateDataTable(IEnumerable<string> ids)
        {
            var table = new DataTable();
            table.Columns.Add("Id", typeof(string));
            foreach (var id in ids)
            {
                table.Rows.Add(id);
            }
            return table;
        }
        private static DataTable CreateDataTable(IEnumerable<Tuple<int,int>> ids)
        {
            var table = new DataTable();
            table.Columns.Add("Id1", typeof(int));
            table.Columns.Add("Id2", typeof(int));

            var idArray = ids as Tuple<int, int>[] ?? ids.ToArray();
            foreach (var id in idArray)
            {
                table.Rows.Add(id.Item1,id.Item2);
            }
            return table;
        }
        private static DataTable CreateDataTable(IEnumerable<Tuple<int, int, int>> ids)
        {
            var table = new DataTable();
            table.Columns.Add("Id1", typeof(int));
            table.Columns.Add("Id2", typeof(int));
            table.Columns.Add("Id3", typeof(int));

            var idArray = ids as Tuple<int, int, int>[] ?? ids.ToArray();
            foreach (var id in idArray)
            {
                table.Rows.Add(id.Item1, id.Item2, id.Item3);
            }
            return table;
        }
    }
}