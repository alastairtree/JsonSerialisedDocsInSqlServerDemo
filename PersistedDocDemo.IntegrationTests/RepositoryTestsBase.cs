using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    public abstract class RepositoryTestsBase<TEntity> where TEntity : new()
    {
        protected TEntity newItem;
        protected IRepository<TEntity> repository;

        [SetUp]
        public virtual void BeforeEachTest()
        {
            repository = BuildRepository();
            repository.DeleteAll();
            newItem = GetNewItem();
        }

        internal abstract IRepository<TEntity> BuildRepository();

        protected abstract TEntity GetNewItem();

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
            var item1 = GetNewItem();
            var item2 = GetNewItem();
            repository.Save(item1);
            repository.Save(item2);
            var allItems = repository.GetAll();
            var ids = allItems.Select(GetId).ToArray();

            Assert.AreEqual(2, allItems.Count);
            Assert.Contains(GetId(item1), ids);
            Assert.Contains(GetId(item2), ids);
        }

        protected abstract object GetId(TEntity item);
    }
}