using NUnit.Framework;
using PersistedDocDemo.Data;
using System.Configuration;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture(typeof(Todo))]
    [TestFixture(typeof(TodoDecoratedWithSerialisable))]
    public class SqlServerRepositoryTodoTests<TodoEntity> : TodoRepositoryTestsBase<TodoEntity> where TodoEntity : Todo, new()
    {
        internal override IRepository<TodoEntity> BuildRepository()
        {
            return new SqlServerRepository<TodoEntity>(ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString);
        }
    }
}