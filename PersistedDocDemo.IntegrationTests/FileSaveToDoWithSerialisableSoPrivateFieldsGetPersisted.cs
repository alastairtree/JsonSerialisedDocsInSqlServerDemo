using System;
using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public class FileSaveToDoWithSerialisableSoPrivateFieldsGetPersisted : RepositoryTestsBase<Todo>
    {
        private readonly Random rand;

        public FileSaveToDoWithSerialisableSoPrivateFieldsGetPersisted()
        {
            rand = new Random();
        }

        protected override object GetId(Todo item)
        {
            return item.Id;
        }

        protected override Todo GetNewItem()
        {
            return new Todo() { Id = rand.Next() };
        }

        [Test]
        public void GetFromTheDatabaseDeserialisesSucessfully()
        {
            newItem = new Todo {Name = "testName", Id=99};
            repository.Save(newItem);
            var databaseValue = repository.Get(GetId(newItem));

            Assert.AreEqual("testName", databaseValue.Name);
        }

        [Test]
        public void SaveToTheDatabaseAssignsNewidentity()
        {
            repository.Save(newItem);
            Assert.Greater(newItem.Id, 0);
        }

        [Test]
        public void SaveTwiceIsAnUpdateWithSameId()
        {
            repository.Save(newItem);
            var idBeffore = GetId(newItem);
            newItem.Name = "update";
            repository.Save(newItem);
            var idAfter = GetId(newItem);
            var items = repository.GetAll();

            Assert.AreEqual(idBeffore, idAfter);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("update", items.Single().Name);
        }

        internal override IRepository<Todo> BuildRepository()
        {
            return new FileRepository<Todo>();
        }
    }
}