using System;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture(typeof(Todo))]
    [TestFixture(typeof(TodoDecoratedWithSerialisable))]
    public class FileRepositortyTests<TodoEntity> : TodoRepositoryTestsBase<TodoEntity> where TodoEntity : Todo, new()
    {
        private readonly Random rand;

        public FileRepositortyTests()
        {
            rand = new Random();
        }

        protected override TodoEntity GetNewItem()
        {
            return new TodoEntity() { Id = rand.Next() };
        }
        internal override IRepository<TodoEntity> BuildRepository()
        {
            return new FileRepository<TodoEntity>();
        }
    }
}