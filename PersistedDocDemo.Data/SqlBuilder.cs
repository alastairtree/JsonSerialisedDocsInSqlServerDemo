using System;
using System.Collections.Generic;

namespace PersistedDocDemo.Data
{
    public class SqlBuilder<TEntity> : ISqlBuilder<TEntity>
    {
        private IRepositoryConfig config;
        private string tableName;

        public void Init(IRepositoryConfig config, string identityFieldName, Dictionary<string, Type> columnMetadata = null)
        {
            if (config == null) throw new ArgumentNullException(nameof(config));
            if (identityFieldName == null) throw new ArgumentNullException(nameof(identityFieldName));

            this.config = config;
            this.IdentityFieldName = identityFieldName;
            this.ColumnMetadata = columnMetadata ?? new Dictionary<string, Type>();
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
        Dictionary<string, Type> ColumnMetadata { get; set; }

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
            columnNames.AddRange(ColumnMetadata.Keys);
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