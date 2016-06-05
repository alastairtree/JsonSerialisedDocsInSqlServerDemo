using System;
using NUnit.Framework;
using PersistedDocDemo.Data;

namespace PersistedDocDemo.IntegrationTests
{
    [TestFixture]
    public class JsonSerialiserTests
    {
        public string SimpleProperty { get; set; }
        public int Id { get; set; }

        [Test]
        public void SerialiseAndDeserialiseSimplePoco()
        {
            var sut = new JsonSerialiser();
            var entity = new JsonSerialiserTests {SimpleProperty = "123"};
            var deserialisedEntity = sut.DeserializeObject<JsonSerialiserTests>(sut.SerializeObject(entity));
            Assert.AreEqual("123", deserialisedEntity.SimpleProperty);
        }

        [Test]
        public void IdColumnsDoNotGetSerialisedWhenIgnored()
        {
            var sut = new JsonSerialiser();
            sut.IgnoreProperty(typeof (JsonSerialiserTests), "Id");
            var entity = new JsonSerialiserTests {SimpleProperty = "123", Id = 1};
            var text = sut.SerializeObject(entity);
            Console.WriteLine(text);
            Assert.AreEqual("{\"simpleProperty\":\"123\"}", text);
        }
    }
}