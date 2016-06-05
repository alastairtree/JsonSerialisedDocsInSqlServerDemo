using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace PersistedDocDemo.IntegrationTests
{
    public abstract class TodoRepositoryTestsBase<TodoEntity> : RepositoryTestsBase<TodoEntity>
        where TodoEntity : Todo, new()
    {
        protected override object GetId(TodoEntity item)
        {
            return item.Id;
        }

        [Test]
        public void SaveChildItemsAndDeserialiseTheProperly()
        {
            newItem.ChildTasks.Add(GetNewItem());
            newItem.ChildTasks.Add(GetNewItem());
            newItem.ChildTasks.Add(GetNewItem());

            repository.Save(newItem);
            var deserialisedChildren = repository.Get(newItem.Id).ChildTasks;

            Assert.AreEqual(3, deserialisedChildren.Count);
        }

        [Test]
        public void GetFromTheRepositoryDeserialisesPropertiesSucessfully()
        {
            newItem.Name = "testName";
            repository.Save(newItem);

            var id = GetId(newItem);
            var storedValue = repository.Get(id);

            Assert.AreEqual("testName", storedValue.Name);
            Assert.AreEqual(id, storedValue.Id);
        }

        [Test]
        public void GetFromTheRepositoryHandlesColumnAttributes()
        {
            newItem.Colour = "Blue";
            repository.Save(newItem);

            var id = GetId(newItem);
            var storedValue = repository.Get(id);

            Assert.AreEqual("Blue", storedValue.Colour);
        }

        [Test]
        public void GetFromTheRepositoryHandlesColumnAttributeOnListOfStrings()
        {
            newItem.Tags = new List<string>(new[] {"Urgent", "Version-1"});

            repository.Save(newItem);

            var id = GetId(newItem);
            var storedValue = repository.Get(id);

            Assert.AreEqual(2, storedValue.Tags.Count);
            Assert.AreEqual("Urgent", storedValue.Tags.First());
            Assert.AreEqual("Version-1", storedValue.Tags.Last());
        }

        [Test]
        public void GetFromTheRepositoryHandlesColumnAttributeOnEmptyListOfStrings()
        {
            newItem.Tags = new List<string>();

            repository.Save(newItem);

            var id = GetId(newItem);
            var storedValue = repository.Get(id);

            Assert.AreEqual(0, storedValue.Tags.Count);
        }

        [Test]
        public void GetAllFromTheRepositoryDeserialisesSucessfully()
        {
            newItem.Name = "testName";
            repository.Save(newItem);

            var id = GetId(newItem);
            var storedValue = repository.GetAll().Single();

            Assert.NotNull(storedValue);
            Assert.AreEqual("testName", storedValue.Name);
            Assert.AreEqual(id, storedValue.Id);
        }

        [Test]
        public void SaveToTheStoreAssignsNewidentity()
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

        protected override TodoEntity GetNewItem()
        {
            return new TodoEntity();
        }
    }
}