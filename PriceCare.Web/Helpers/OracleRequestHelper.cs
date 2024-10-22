using Oracle.ManagedDataAccess.Client;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace PriceCare.Web.Helpers
{
    public class OracleRequestHelper
    {
        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<DataRow, T> convertRow, Dictionary<string, object> parameters)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                foreach (var parameter in parameters)
                {
                    command.Parameters.Add(parameter.Key, parameter.Value);
                }

                var adapter = new OracleDataAdapter(command);
                var table = new DataTable();
                adapter.Fill(table);

                foreach (DataRow row in table.Rows)
                {
                    yield return convertRow(row);
                }
            }
        }

        public static IEnumerable<T> ExecuteQuery<T>(string connectionString, string commandText, Func<IDataReader, T> convertRow)
        {
            using (var connection = new OracleConnection(connectionString))
            {
                var command = connection.CreateCommand();
                command.CommandText = commandText;

                connection.Open();
                try
                {
                    OracleDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        yield return convertRow(reader);
                    }
                }
                finally
                {
                    connection.Close();
                }
            }
        }
    }
}