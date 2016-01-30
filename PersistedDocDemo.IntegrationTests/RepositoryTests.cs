using System.Configuration;
using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public abstract class RepositoryTests<TEntity> where TEntity : new()
    {
        [SetUp]
        public virtual void BeforeEachTest()
        {
            repository =
                new SqlServerRepository<TEntity>(ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString);
            repository.DeleteAll();
            newItem = GetNewItem();
        }

        protected TEntity newItem;
        protected SqlServerRepository<TEntity> repository;

        protected TEntity GetNewItem()
        {
            return new TEntity();;
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
            var result = repository.Delete(GetId(newItem));

            Assert.True(result);
            Assert.IsNull(repository.Get(GetId(newItem)));
        }

        [Test]
        public void DeleteByValueReturnsTrueAndHasDeletedTheRow()
        {
            repository.Save(newItem);
            var result = repository.Delete(newItem);

            Assert.True(result);
            Assert.IsNull(repository.Get(GetId(newItem)));
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
        public void SaveTwiceAndGetAllReturnResults()
        {
            repository.Save(GetNewItem());
            repository.Save(GetNewItem());
            var allItems = repository.GetAll();

            Assert.AreEqual(2, allItems.Count);
        }      

        protected abstract object GetId(TEntity item);
    }
}