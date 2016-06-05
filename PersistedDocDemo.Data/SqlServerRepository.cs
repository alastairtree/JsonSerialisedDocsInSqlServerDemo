using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PersistedDocDemo.Data
{
    public class SqlServerRepository<T> : RepositoryBase<T>
    {
        private readonly IDatabase database;
        private readonly ISqlBuilder<T> sqlBuilder;
        private static readonly List<string> CollectionColumns = new List<string>();
        private static Dictionary<string, Type> indexedColumnsInfo = null;
        private static string delimiter = "|";

        static SqlServerRepository()
        {
            InitColumnMapping();
        }

        public SqlServerRepository(IEntitySerialiser serialiser, IRepositoryConfig config, IDatabase database, ISqlBuilder<T> sqlBuilder)
        {
            this.database = database;
            Serialiser = serialiser;
            Config = config;

            this.sqlBuilder = sqlBuilder;
            sqlBuilder.Init(Config,IdentityFieldName,indexedColumnsInfo);
            InitSerialiser();

        }

        public SqlServerRepository() : this(new JsonSerialiser(), new DefaultRepositoryConfig(), new SqlServer(), new SqlBuilder<T>())
        {
        }

        public SqlServerRepository(string connectionString)
            : this()
        {
            database.ConnectionString = connectionString;
        }


        public IRepositoryConfig Config { get; }

        private static void InitColumnMapping()
        {
            var columnNames = GetPropertyNameByCustomAttribute<T, SqlColumnAttribute>() ?? new string[0];
            indexedColumnsInfo = columnNames.ToDictionary(x => x, GetMemberType);

            foreach (var columnName in indexedColumnsInfo.Keys)
            {
                var actualType = indexedColumnsInfo[columnName];
                if (IsEnumerable(actualType) && actualType != typeof(string))
                {
                    CollectionColumns.Add(columnName);
                }
            }
        }

        private void InitSerialiser()
        {
            if (!string.IsNullOrEmpty(IdentityFieldName))
            {
                // ignore the id property compiler generated backing field if needed
                if (typeof(T).IsSerializable)
                {
                    var backingFieldDeclaringType = typeof(T).GetMember(IdentityFieldName)[0].DeclaringType;
                    Serialiser.IgnoreProperty(backingFieldDeclaringType, $"<{IdentityFieldName}>k__BackingField");
                }
                else
                {
                    Serialiser.IgnoreProperty(typeof(T), IdentityFieldName);
                }
            }

            //ignore properties stored in proper columns
            foreach (var sqlColumn in indexedColumnsInfo.Keys)
            {
                var memberInfo = typeof(T).GetMember(sqlColumn)[0];
                if (typeof(T).IsSerializable) //we need to ignore the backing field 
                {
                    Serialiser.IgnoreProperty(memberInfo.DeclaringType, $"<{sqlColumn}>k__BackingField");
                }
                else //otherwise just ignore the property
                {
                    Serialiser.IgnoreProperty(typeof(T), sqlColumn);
                }
            }
        }

        static Type GetMemberType(string memberName)
        {
            var memberInfo = typeof(T).GetMember(memberName)[0];
            return GetUnderlyingType(memberInfo);
        }

        static bool IsEnumerable(Type type)
        {
            return type.IsArray || typeof(IEnumerable).IsAssignableFrom(type) || type.GetInterface(typeof(IEnumerable<>).FullName) != null;

        }

        static Type GetUnderlyingType(MemberInfo member)
        {

            switch (member.MemberType)
            {
                case MemberTypes.Event:
                    return ((EventInfo)member).EventHandlerType;
                case MemberTypes.Field:
                    return ((FieldInfo)member).FieldType;
                case MemberTypes.Method:
                    return ((MethodInfo)member).ReturnType;
                case MemberTypes.Property:
                    return ((PropertyInfo)member).PropertyType;
                default:
                    throw new ArgumentException
                    (
                     "Input MemberInfo must be if type EventInfo, FieldInfo, MethodInfo, or PropertyInfo"
                    );
            }
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

            foreach (var nonEnumerableColumn in indexedColumnsInfo.Keys.Except(CollectionColumns))
            {
                SetProperty(value, nonEnumerableColumn, row[nonEnumerableColumn]);
            }

            foreach (var enumerableColumn in indexedColumnsInfo.Keys.Intersect(CollectionColumns))
            {
                var values = row[enumerableColumn].ToString();

                var areNumbers = Regex.IsMatch(@"(\d+,?)+", values);
                var valuesAsJson = "";
                if (areNumbers)
                    valuesAsJson = "[" +values.Replace(delimiter, ",")  + "]";
                else
                    valuesAsJson = "[\"" + values.Replace(delimiter, "\",\"") + "\"]";


                var enumerableColumnValue = JsonSerialiser.DeserializeObject(valuesAsJson,
                    indexedColumnsInfo[enumerableColumn]);

                SetProperty(value, enumerableColumn, enumerableColumnValue);
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
                var item = DeserialiseRow(row);
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
            foreach (var sqlColumn in indexedColumnsInfo.Keys)
            {
                var value = GetValueFromProperty(item, sqlColumn) ?? DBNull.Value;

                value = ConvertCollectionsToText(value);

                parameters.Add(Tuple.Create(sqlColumn,value));
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

        private static object ConvertCollectionsToText(object value)
        {
            var enumerable = value as ICollection;
            if (enumerable != null)
            {
                if (enumerable.Count > 0)
                {
                    //convert the value into a pipe seperated string
                    value = string.Join(delimiter, enumerable.Cast<object>()
                        .Select(x => x.ToString()));
                }
                else
                {
                    value = "";
                }
            }
            return value;
        }
    }
}