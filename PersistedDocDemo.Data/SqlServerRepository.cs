using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace PersistedDocDemo.Data
{
    public class SqlServerRepository<T> : RepositoryBase<T>
    {
        private readonly IDatabase database;
        private readonly ISqlBuilder<T> sqlBuilder;
        private readonly List<string> CollectionColumns = new List<string>();
        private Dictionary<string, Type> indexedColumnMetadata = null;


        public SqlServerRepository(IEntitySerialiser serialiser, IRepositoryConfig config, IDatabase database, ISqlBuilder<T> sqlBuilder)
        {
            InitColumnMapping();

            this.database = database;
            Serialiser = serialiser;
            Config = config;

            this.sqlBuilder = sqlBuilder;
            sqlBuilder.Init(Config,IdentityFieldName,indexedColumnMetadata);
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

        public Dictionary<string, Type> IndexedColumnMetadata
        {
            get { return indexedColumnMetadata; }
        }

        private void InitColumnMapping()
        {
            if (indexedColumnMetadata == null)
            {
                var type = typeof (T).FullName;
                var columnNames = GetPropertyNameByCustomAttribute<T, SqlColumnAttribute>() ?? new string[0];
                indexedColumnMetadata = columnNames.ToDictionary(x => x, GetMemberType);

                foreach (var columnName in indexedColumnMetadata.Keys)
                {
                    var actualType = indexedColumnMetadata[columnName];

                    Debug.WriteLine($"{type} column mapping for {columnName} is {actualType.FullName}");

                    if (IsEnumerable(actualType) && actualType != typeof (string))
                    {
                        CollectionColumns.Add(columnName);
                        Debug.Write(" and is enumerable");
                    }
                }
            }
        }

        private void InitSerialiser()
        {
            if(indexedColumnMetadata == null) throw new NotSupportedException("Mapping has not been set up");

            if (!string.IsNullOrEmpty(IdentityFieldName))
            {
                // ignore the id property compiler generated backing field if needed
                IgnoreProperty(IdentityFieldName);
            }

            //ignore properties stored in proper columns
            foreach (var sqlColumn in indexedColumnMetadata.Keys)
            {
                IgnoreProperty(sqlColumn);
            }
        }

        private void IgnoreProperty(string sqlColumn)
        {
            if (typeof (T).IsSerializable) //we need to ignore the backing field 
            {
                var backingFieldDeclaringType = typeof(T).GetMember(sqlColumn)[0].DeclaringType;
                Serialiser.IgnoreProperty(backingFieldDeclaringType, $"<{sqlColumn}>k__BackingField");
                Debug.WriteLine($"Instruction serialiser to ignore property {$"<{sqlColumn}>k__BackingField"} on type { backingFieldDeclaringType }");

            }
            else //otherwise just ignore the property
            {
                Serialiser.IgnoreProperty(typeof (T), sqlColumn);
                Debug.WriteLine($"Instructing serialiser to ignore property {sqlColumn} on type { typeof(T).Name }");

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

            foreach (var nonEnumerableColumn in indexedColumnMetadata.Keys.Except(CollectionColumns))
            {
                SetProperty(value, nonEnumerableColumn, row[nonEnumerableColumn]);
            }

            foreach (var enumerableColumn in indexedColumnMetadata.Keys.Intersect(CollectionColumns))
            {
                var values = row[enumerableColumn].ToString().Trim(Config.ColumnItemsDelimeter.ToCharArray());

                var areNumbers = Regex.IsMatch(@"(\d+,?)+", values);
                var valuesAsJson = "";
                if (areNumbers)
                    valuesAsJson = "[" +values.Replace(Config.ColumnItemsDelimeter, ",")  + "]";
                else
                    valuesAsJson = "[\"" + values.Replace(Config.ColumnItemsDelimeter, "\",\"") + "\"]";


                var enumerableColumnValue = new JsonSerialiser().DeserializeObject(valuesAsJson,
                    indexedColumnMetadata[enumerableColumn]);

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
            foreach (var sqlColumn in indexedColumnMetadata.Keys)
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

        private object ConvertCollectionsToText(object value)
        {
            var enumerable = value as ICollection;
            if (enumerable != null)
            {
                if (enumerable.Count > 0)
                {
                    //convert the value into a pipe seperated string
                    value = Config.ColumnItemsDelimeter +  string.Join(Config.ColumnItemsDelimeter, enumerable.Cast<object>()
                        .Select(x => x.ToString())) + Config.ColumnItemsDelimeter;
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