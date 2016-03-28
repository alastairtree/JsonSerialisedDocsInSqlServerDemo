using System;
using System.Collections.Generic;
using System.Data;

namespace PersistedDocDemo.Data
{
    public class SqlServerRepository<T> : RepositoryBase<T>
    {
        private readonly IDatabase database;
        private readonly SqlBuilder<T> sqlBuilder;

        public SqlServerRepository(IEntitySerialiser serialiser, IRepositoryConfig config, IDatabase database)
        {
            this.database = database;
            Serialiser = serialiser;
            Config = config;
            var identityFieldName = GetIdentityFieldName();
            if (!string.IsNullOrEmpty(IdentityFieldName))
                serialiser.IgnoreProperty(typeof (T), identityFieldName);
            sqlBuilder = new SqlBuilder<T>(Config) {IdentityFieldName = IdentityFieldName};
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

        public override T Get(object id)
        {
            var sql = sqlBuilder.SelectByIdSql();

            var data = database.ExecuteSqlTableQuery(sql, Tuple.Create("id", id));
            var value = default(T);

            if (data.Rows.Count == 1)
            {
                value = DeserialiseRow(data.Rows[0]);
            }

            return value;
        }

        private T DeserialiseRow(DataRow row)
        {
            var value = Serialiser.DeserializeObject<T>(row[1]);
            SetIdentity(value, row[0]);
            return value;
        }

        public override ICollection<T> GetAll()
        {
            var sql = sqlBuilder.SelectAllSql();

            var data = database.ExecuteSqlTableQuery(sql);

            var results = new List<T>();

            foreach (DataRow row in data.Rows)
            {
                var item = DeserialiseRow(data.Rows[0]);
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
                sql = sqlBuilder.InsertSql();
            }
            else
            {
                sql = sqlBuilder.UpdateSql();
            }

            var serialisedData = Serialiser.SerializeObject(item);
            id = database.ExecuteSqlScalar(sql, Tuple.Create("Data", serialisedData)) ?? id;

            SetIdentity(item, id);
        }

        public override bool Delete(object id)
        {
            var sql = sqlBuilder.DeleteByIdSql();
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
            var sql = sqlBuilder.DeleteSql();
            var rows = database.ExecuteNonQuery(sql);
            return rows > 0;
        }
    }
}