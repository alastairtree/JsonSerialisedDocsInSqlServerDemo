using System;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;

namespace PersistedDocDemo.Data
{
    public class SqlServer : IDatabase
    {
        public string ConnectionString { get; set; }

        public object ExecuteSqlScalar(string sql, params Tuple<string, object>[] parameters)
        {
            object value;
            using (var conn = new SqlConnection(ConnectionString))
            {
                var command = CreateCommand(sql, parameters, conn);
                value = command.ExecuteScalar();
            }
            return value;
        }

        public int ExecuteNonQuery(string sql, params Tuple<string, object>[] parameters)
        {
            LogSqL(sql, parameters);

            int value;
            using (var conn = new SqlConnection(ConnectionString))
            {
                var command = CreateCommand(sql, parameters, conn);
                value = command.ExecuteNonQuery();
            }
            return value;
        }

        public DataTable ExecuteSqlTableQuery(string sql, params Tuple<string, object>[] parameters)
        {
            LogSqL(sql, parameters);

            using (var conn = new SqlConnection(ConnectionString))
            {
                var command = CreateCommand(sql, parameters, conn);
                var adapter = new SqlDataAdapter(command);
                var dataTable = new DataTable();
                adapter.Fill(dataTable);
                return dataTable;
            }
        }

        private static void LogSqL(string sql, Tuple<string, object>[] parameters)
        {
            Debug.WriteLine("Executing SQL command: " + sql);
            foreach (var parameter in parameters)
            {
                Debug.WriteLine($"{parameter.Item1} = {parameter.Item2}");
            }
        }

        private static SqlCommand CreateCommand(string sql, Tuple<string, object>[] parameters, SqlConnection conn)
        {
            LogSqL(sql, parameters);

            var command = new SqlCommand(sql, conn);

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    command.Parameters.AddWithValue(parameter.Item1, parameter.Item2);
                }
            }

            conn.Open();
            return command;
        }
    }
}