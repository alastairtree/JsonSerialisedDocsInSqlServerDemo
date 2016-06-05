using System.Collections.Generic;

namespace PersistedDocDemo.Data
{
    public class SqlBuilder<TEntity>
    {
        private readonly IRepositoryConfig config;
        private string tableName;

        public SqlBuilder(IRepositoryConfig config)
        {
            this.config = config;
            SqlColumns = new List<string>();
        }

        public string TableName
        {
            get
            {
                if (tableName == null)
                {
                    tableName = $"[{config.DatabaseSchemaName}].[{typeof (TEntity).Name}]";
                }
                return tableName;
            }
            set { tableName = value; }
        }

        public string IdentityFieldName { get; set; }
        public ICollection<string> SqlColumns { get; set; }

        public string SelectByIdSql()
        {
            return $"SELECT {GenerateColumns()} FROM {TableName} WHERE [{IdentityFieldName}] = @id";
        }

        public string SelectAllSql()
        {
            return $"SELECT {GenerateColumns()} FROM {TableName}";
        }

        public string UpdateSql()
        {
            var setters = GenerateColumns(true, "[{0}]=@{0}");
            return $"UPDATE {TableName} SET {setters};";
        }

        public string InsertSql()
        {
            var sqlColumns = GenerateColumns(true);
            var sqlColumnParams = GenerateColumns(true, "@{0}");
            return $"INSERT {TableName} ({sqlColumns}) VALUES ({sqlColumnParams}); SELECT SCOPE_IDENTITY();";
        }

        private string GenerateColumns(bool surpressId = false, string format = "[{0}]")
        {
            var columnNames = new List<string>();
            if (!surpressId)
                columnNames.Add(IdentityFieldName);
            columnNames.AddRange(SqlColumns);
            columnNames.Add("Data");

            var text = string.Empty;
            for (var i = 0; i < columnNames.Count; i++)
            {
                if (!string.IsNullOrEmpty(columnNames[i]))
                    text += string.Format(format, columnNames[i]);
                if (i != columnNames.Count - 1)
                {
                    text += ",";
                }
            }
            return text;
        }

        public string DeleteByIdSql()
        {
            return $"DELETE FROM {TableName} WHERE [{IdentityFieldName}] = @id";
        }

        public string DeleteSql()
        {
            return $"DELETE FROM {TableName}";
        }
    }
}