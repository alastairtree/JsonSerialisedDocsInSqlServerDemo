using System;
using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;
using System.Configuration;

namespace PersistedDocDemo.IntegrationTests
{
    [Serializable]
    public class Todo
    {
        private DateTime created = DateTime.UtcNow;
        public int Id { get; set; }
        public string Name { get; set; }
    }


    [TestFixture]
    public class SqlSaveToDoWithSerialisableSoPrivateFieldsGetPersisted: RepositoryTestsBase<Todo>
    {
        protected override object GetId(Todo item)
        {
            return item.Id;
        }

        [Test]
        public void GetFromTheDatabaseDeserialisesSucessfully()
        {
            newItem = new Todo {Name = "testName"};
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
            return new SqlServerRepository<Todo>(ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString);
        }

        protected override Todo GetNewItem()
        {
            return new Todo();
        }
    }
}