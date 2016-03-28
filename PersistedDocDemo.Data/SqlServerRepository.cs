using System;
using System.Collections.Generic;
using System.Data;

namespace PersistedDocDemo.Data
{
    public class SqlServerRepository<T> : RepositoryBase<T>
    {
        private readonly IDatabase database;
        private string tableName;

        public SqlServerRepository(IEntitySerialiser serialiser, IRepositoryConfig config, IDatabase database)
        {
            this.database = database;
            Serialiser = serialiser;
            Config = config;
            var identityFieldName = GetIdentityFieldName();
            if(!string.IsNullOrEmpty(IdentityFieldName))
                serialiser.IgnoreProperty(typeof(T), identityFieldName);
        }

        public SqlServerRepository() : this(new JsonSerialiser(), new DefaultRepositoryConfig(), new SqlServer())
        {
        }

        public SqlServerRepository(string connectionString)
            : this()
        {
            database.ConnectionString = connectionString;
        }

        public IRepositoryConfig Config { get; }

        public string TableName
        {
            get
            {
                if (tableName == null)
                {
                    tableName = string.Format("[{0}].[{1}]", Config.DatabaseSchemaName, typeof (T).Name);
                }
                return tableName;
            }
            set { tableName = value; }
        }

        public override T Get(object id)
        {
            var sql = string.Format("SELECT [Data] FROM {0} WHERE [{1}] = @id", TableName, IdentityFieldName);

            var data = database.ExecuteSqlScalar(sql, Tuple.Create("id", id));
            var value = default(T);

            if (data != null)
            {
                value = Serialiser.DeserializeObject<T>(data);
                SetIdentity(value, id);
            }

            return value;
        }

        public override ICollection<T> GetAll()
        {
            var sql = string.Format("SELECT {0}, [Data] FROM {1}", IdentityFieldName, TableName);

            var data = database.ExecuteSqlTableQuery(sql);

            var results = new List<T>();

            foreach (DataRow row in data.Rows)
            {
                var item = Serialiser.DeserializeObject<T>(row[1]);
                SetIdentity(item, row[0]);
                results.Add(item);
            }

            return results;
        }

        public override void Save(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            var id = GetIdentityValue(item);
            string sql;
            if (IsUndefinedKey(id))
            {
                sql = string.Format("INSERT {0} ([Data]) VALUES (@data); SELECT SCOPE_IDENTITY();", TableName);
            }
            else
            {
                sql = string.Format("UPDATE {0} SET [Data] = @data;", TableName);
            }

            var serialisedData = Serialiser.SerializeObject(item);
            id = database.ExecuteSqlScalar(sql, Tuple.Create("Data", serialisedData)) ?? id;

            SetIdentity(item, id);
        }

        public override bool Delete(object id)
        {
            var sql = string.Format("DELETE FROM {0} WHERE [{1}] = @id", TableName, IdentityFieldName);
            var rows = database.ExecuteNonQuery(sql, Tuple.Create("id", id));
            return rows > 0;
        }

        public override bool Delete(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var id = GetIdentityValue(item);
            return Delete(id);
        }

        public override bool DeleteAll()
        {
            var rows = database.ExecuteNonQuery(string.Format("DELETE FROM {0}", TableName));
            return rows > 0;
        }
    }
}