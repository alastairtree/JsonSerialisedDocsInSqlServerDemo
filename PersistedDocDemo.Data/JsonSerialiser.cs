using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PersistedDocDemo.Data
{
    public class JsonSerialiser : IEntitySerialiser
    {
        public JsonSerialiser()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            ContractResolver = new IgnorablePropertyCamelCaseNamesContractResolver() {IgnoreSerializableAttribute = false};
            JsonSerializerSettings.ContractResolver = ContractResolver;
        }

        public IgnorablePropertyCamelCaseNamesContractResolver ContractResolver { get; set; }

        public JsonSerializerSettings JsonSerializerSettings { set; get; }

        public TEntity DeserializeObject<TEntity>(object data)
        {
            return JsonConvert.DeserializeObject<TEntity>(data.ToString(), JsonSerializerSettings);
        }

        public object SerializeObject<TEntity>(TEntity item)
        {
            return JsonConvert.SerializeObject(item, JsonSerializerSettings);
        }

        /// <summary>
        /// Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void IgnoreProperty(Type type, params string[] propertyName)
        {
            ContractResolver.Ignore(type, propertyName);
        }
    }
}