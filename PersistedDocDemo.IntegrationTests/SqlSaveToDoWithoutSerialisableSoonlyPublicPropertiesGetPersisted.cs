﻿using System;
using System.Linq;
using NUnit.Framework;
using PersistedDocDemo.Data;
using System.Configuration;

namespace PersistedDocDemo.IntegrationTests.PublicProperties
{
    public class Task
    {
        public int TaskId { get; set; }
        public string TaskName { get; set; }
    }

    [TestFixture]
    public class SqlSaveToDoWithoutSerialisableSoonlyPublicPropertiesGetPersisted: RepositoryTestsBase<Task>
    {
        protected override object GetId(Task item)
        {
            return item.TaskId;
        }

        [Test]
        public void GetFromTheDatabaseDeserialisesSucessfully()
        {
            newItem.TaskName = "testName";
            repository.Save(newItem);
            var databaseValue = repository.Get(GetId(newItem));

            Assert.AreEqual("testName", databaseValue.TaskName);
        }



        [Test]
        public void SaveToTheDatabaseAssignsNewidentity()
        {
            repository.Save(newItem);
            Assert.Greater(newItem.TaskId, 0);
        }

        [Test]
        public void SaveTwiceIsAnUpdateWithSameId()
        {
            repository.Save(newItem);
            var idBeffore = GetId(newItem);
            newItem.TaskName = "update";
            repository.Save(newItem);
            var idAfter = GetId(newItem);
            var items = repository.GetAll();

            Assert.AreEqual(idBeffore, idAfter);
            Assert.AreEqual(1, items.Count);
            Assert.AreEqual("update", items.Single().TaskName);
        }

        internal override IRepository<Task> BuildRepository()
        {
            return new SqlServerRepository<Task>(ConfigurationManager.ConnectionStrings["PersistedDb"].ConnectionString);
        }

        protected override Task GetNewItem()
        {
            return new Task();
        }
    }
}