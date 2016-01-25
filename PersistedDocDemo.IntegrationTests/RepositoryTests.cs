using System.Configuration;
using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public class RepositoryTests
    {
        [SetUp]
        public void BeforeEachTest()
        {
            repository =
                new SqlServerRepository<Todo>(ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString);
            repository.DeleteAll();
            newItem = GetNewItem();
        }

        private Todo newItem;
        private SqlServerRepository<Todo> repository;

        private static Todo GetNewItem()
        {
            return new Todo {Name = "testName"};
        }

        [Test]
        public void DeleteAllEntriesTheTable()
        {
            //DeleteAll() in Setup
            var allItems = repository.GetAll();

            Assert.AreEqual(0, allItems.Count);
        }

        [Test]
        public void DeleteByIdReturnsTrueAndHasDeletedTheRow()
        {
            repository.Save(newItem);
            var result = repository.Delete(newItem.Id);

            Assert.True(result);
            Assert.IsNull(repository.Get(newItem.Id));
        }

        [Test]
        public void DeleteByValueReturnsTrueAndHasDeletedTheRow()
        {
            repository.Save(newItem);
            var result = repository.Delete(newItem);

            Assert.True(result);
            Assert.IsNull(repository.Get(newItem.Id));
        }

        [Test]
        public void DeleteNothingReturnsFalse()
        {
            var result = repository.DeleteAll();

            Assert.False(result);
        }

        [Test]
        public void DeleteOnlyRowReturnsTrue()
        {
            repository.Save(newItem);
            var result = repository.DeleteAll();

            Assert.True(result);
        }

        [Test]
        public void GetAllReturnsManyResults()
        {
            repository.Save(GetNewItem());
            repository.Save(GetNewItem());
            var allItems = repository.GetAll();

            Assert.AreEqual(2, allItems.Count);
        }

        [Test]
        public void GetFromTheDatabaseDeserialisesSucessfully()
        {
            repository.Save(newItem);
            var databaseValue = repository.Get(newItem.Id);

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
            var idBeffore = newItem.Id;
            newItem.Name = "update";
            repository.Save(newItem);
            var idAfter = newItem.Id;
            var items = repository.GetAll();

            Assert.AreEqual(idBeffore, idAfter);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("update", items.Single().Name);
        }
    }
}