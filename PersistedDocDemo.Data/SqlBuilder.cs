namespace PersistedDocDemo.Data
{
    internal class SqlBuilder<TEntity>
    {
        private readonly IRepositoryConfig config;
        private string tableName;

        public SqlBuilder(IRepositoryConfig config)
        {
            this.config = config;
        }

        public string TableName
        {
            get
            {
                if (tableName == null)
                {
                    tableName = string.Format("[{0}].[{1}]", config.DatabaseSchemaName, typeof (TEntity).Name);
                }
                return tableName;
            }
            set { tableName = value; }
        }

        public string IdentityFieldName { get; set; }

        public string SelectByIdSql()
        {
            return string.Format("SELECT {0}, [Data] FROM {1} WHERE [{0}] = @id", IdentityFieldName, TableName);
        }

        public string SelectAllSql()
        {
            return string.Format("SELECT {0}, [Data] FROM {1}", IdentityFieldName, TableName);
        }

        public string UpdateSql()
        {
            return string.Format("UPDATE {0} SET [Data] = @data;", TableName);
        }

        public string InsertSql()
        {
            return string.Format("INSERT {0} ([Data]) VALUES (@data); SELECT SCOPE_IDENTITY();", TableName);
        }

        public string DeleteByIdSql()
        {
            return string.Format("DELETE FROM {0} WHERE [{1}] = @id", TableName, IdentityFieldName);
        }

        public string DeleteSql()
        {
            return string.Format("DELETE FROM {0}", TableName);
        }
    }
}