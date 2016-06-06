using System;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public class JsonSerialiserTests
    {
        private JsonSerialiser jsonSerialiser;

        [SetUp]
        public void JsonSerialiserTestsSetup()
        {
            jsonSerialiser = new JsonSerialiser();
        }

        public string SimpleProperty { get; set; }
        public int Id { get; set; }

        [Test]
        public void SerialiseAndDeserialiseSimplePoco()
        {
            var entity = new JsonSerialiserTests {SimpleProperty = "123"};
            var deserialisedEntity = jsonSerialiser.DeserializeObject<JsonSerialiserTests>(jsonSerialiser.SerializeObject(entity));
            Assert.AreEqual("123", deserialisedEntity.SimpleProperty);
        }

        [Test]
        public void IdColumnsDoNotGetSerialisedWhenIgnored()
        {
            jsonSerialiser.IgnoreProperty(typeof (JsonSerialiserTests), "Id");
            var entity = new JsonSerialiserTests {SimpleProperty = "123", Id = 1};
            var text = jsonSerialiser.SerializeObject(entity);
            Assert.AreEqual("{\"simpleProperty\":\"123\"}", text);
        }

        [Test]
        public void ItsOkToIgnoreMultipleProperties()
        {
            jsonSerialiser.IgnoreProperty(typeof(JsonSerialiserTests), "Id");
            jsonSerialiser.IgnoreProperty(typeof(JsonSerialiserTests), "SimpleProperty");

            var entity = new JsonSerialiserTests { SimpleProperty = "123", Id = 1 };
            var text = jsonSerialiser.SerializeObject(entity);
            Assert.AreEqual("{}", text);
        }

        [Test]
        public void ItsOkToIgnoreMultiplePropertiesInOneGo()
        {
            jsonSerialiser.IgnoreProperty(typeof(JsonSerialiserTests), "Id", "SimpleProperty");

            var entity = new JsonSerialiserTests { SimpleProperty = "123", Id = 1 };
            var text = jsonSerialiser.SerializeObject(entity);
            Assert.AreEqual("{}", text);
        }
    }
}