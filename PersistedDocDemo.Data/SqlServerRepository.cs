using System;
using System.Collections.Generic;
using System.Data;

namespace PersistedDocDemo.Data
{
    public class SqlServerRepository<T> : RepositoryBase<T>
    {
        private readonly IDatabase database;
        private SqlBuilder<T> sqlBuilder;

        public SqlServerRepository(IEntitySerialiser serialiser, IRepositoryConfig config, IDatabase database)
        {
            this.database = database;
            Serialiser = serialiser;
            Config = config;

            InitIdentityMapping();
            InitColumnMapping();
        }

        public SqlServerRepository() : this(new JsonSerialiser(), new DefaultRepositoryConfig(), new SqlServer())
        {
        }

        public SqlServerRepository(string connectionString)
            : this()
        {
            database.ConnectionString = connectionString;
        }

        public ICollection<string> SqlColumns { get; set; }

        public IRepositoryConfig Config { get; }

        private void InitColumnMapping()
        {
            SqlColumns = GetPropertyNameByCustomAttribute<T, SqlColumnAttribute>() ?? new string[0];

            foreach (var sqlColumn in SqlColumns)
            {
                // ignore the column property compiler generated backing field if needed
                if (typeof (T).IsSerializable)
                {
                    var backingFieldDeclaringType = typeof (T).GetMember(sqlColumn)[0].DeclaringType;
                    Serialiser.IgnoreProperty(backingFieldDeclaringType, $"<{sqlColumn}>k__BackingField");
                }
                else // ignore the property
                {
                    Serialiser.IgnoreProperty(typeof (T), sqlColumn);
                }
            }

            sqlBuilder.SqlColumns = SqlColumns;
        }

        private void InitIdentityMapping()
        {
            var identityFieldName = GetIdentityFieldName();

            if (!string.IsNullOrEmpty(IdentityFieldName))
            {
                // ignore the id property compiler generated backing field if needed
                if (typeof (T).IsSerializable)
                {
                    var backingFieldDeclaringType = typeof (T).GetMember(IdentityFieldName)[0].DeclaringType;
                    Serialiser.IgnoreProperty(backingFieldDeclaringType, $"<{identityFieldName}>k__BackingField");
                }
                else
                {
                    Serialiser.IgnoreProperty(typeof (T), identityFieldName);
                }
            }

            sqlBuilder = new SqlBuilder<T>(Config) {IdentityFieldName = IdentityFieldName};
        }

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
            var value = Serialiser.DeserializeObject<T>(row["Data"]);
            if (value != null)
                SetIdentity(value, row[IdentityFieldName]);

            foreach (var sqlColumn in SqlColumns)
            {
                SetProperty(value, sqlColumn, row[sqlColumn]);
            }

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

            var parameters = new List<Tuple<string, object>>();
            foreach (var sqlColumn in SqlColumns)
            {
                parameters.Add(Tuple.Create(sqlColumn, GetValueFromProperty(item, sqlColumn) ?? DBNull.Value));
            }
            parameters.Add(Tuple.Create("Data", serialisedData));

            id = database.ExecuteSqlScalar(sql, parameters.ToArray()) ?? id;

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