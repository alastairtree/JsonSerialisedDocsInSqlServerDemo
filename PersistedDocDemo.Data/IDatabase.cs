using System;
using System.Data;

namespace PersistedDocDemo.Data
{
    public interface IDatabase
    {
        string ConnectionString { get; set; }
        object ExecuteSqlScalar(string sql, params Tuple<string, object>[] parameters);
        int ExecuteNonQuery(string sql, params Tuple<string, object>[] parameters);
        DataTable ExecuteSqlTableQuery(string sql, params Tuple<string, object>[] parameters);
    }
}