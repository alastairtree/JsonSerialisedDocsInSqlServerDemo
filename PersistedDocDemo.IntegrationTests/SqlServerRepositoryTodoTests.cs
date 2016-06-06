using System;
using System.Collections;
using System.Configuration;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public class SqlServerRepositoryTodoTests : SqlServerRepositoryTodoTests<Todo> { }

    [TestFixture]
    public class SqlServerRepositoryTodoDecoratedWithSerialisableTests : SqlServerRepositoryTodoTests<TodoDecoratedWithSerialisable> { }

    public class SqlServerRepositoryTodoTests<TodoEntity> : TodoRepositoryTestsBase<TodoEntity>
        where TodoEntity : Todo, new()
    {

        private string connectionString;
        private SqlServer database;
        private SqlBuilder<TodoEntity> sqlBuilder;
        private SqlServerRepository<TodoEntity> sqlServerRepository;

        [SetUp]
        public override void BeforeEachTest()
        {
            base.BeforeEachTest();
            sqlBuilder = new SqlBuilder<TodoEntity>();
            sqlBuilder.Init(new DefaultRepositoryConfig() { ConnectionString = connectionString }, "Id");
            database = new SqlServer { ConnectionString = connectionString };
        }

        internal override IRepository<TodoEntity> BuildRepository()
        {
            connectionString = ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString;
            sqlServerRepository = new SqlServerRepository<TodoEntity>(connectionString);
            return sqlServerRepository;
        }

        [Test]
        public void IdColumnValueDoesNotGetSavedToJsonDataColumn()
        {
            var item = GetNewItem();
            repository.Save(item);

            var sql = sqlBuilder.SelectByIdSql();
            var data = database.ExecuteSqlTableQuery(sql, Tuple.Create<string, object>("id", item.Id));
            var json = data.Rows[0]["data"] as string;

            Assert.NotNull(json);
            Assert.False(json.Contains("\"id\":"));
            Assert.False(json.Contains("\"<Id>k__BackingField\":"));
        }

        [Test]
        public void SqlColumnDecoratedPropertiesDontGetSavedToJsonDataColumn()
        {
            var item = GetNewItem();
            item.Colour = "Red";
            repository.Save(item);

            var sql = sqlBuilder.SelectByIdSql();
            var data = database.ExecuteSqlTableQuery(sql, Tuple.Create<string, object>("id", item.Id));
            var json = data.Rows[0]["data"] as string;

            Assert.NotNull(json);
            Assert.False(json.Contains("Red"));
        }

    }
}   