using System;
using Newtonsoft.Json;

namespace PersistedDocDemo.Data
{
    public class JsonSerialiser : IEntitySerialiser
    {
       static JsonSerialiser()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            ContractResolver = new IgnorablePropertyCamelCaseNamesContractResolver {IgnoreSerializableAttribute = false};
            JsonSerializerSettings.ContractResolver = ContractResolver;
        }

        public static IgnorablePropertyCamelCaseNamesContractResolver ContractResolver { get; set; }

        public static JsonSerializerSettings JsonSerializerSettings { set; get; }

        public TEntity DeserializeObject<TEntity>(object data)
        {
            return JsonConvert.DeserializeObject<TEntity>(data.ToString(), JsonSerializerSettings);
        }

        internal static object DeserializeObject(object data, Type type)
        {
            return JsonConvert.DeserializeObject(data.ToString(), type, JsonSerializerSettings);
        }

        public object SerializeObject<TEntity>(TEntity item)
        {
            return JsonConvert.SerializeObject(item, JsonSerializerSettings);
        }

        /// <summary>
        ///     Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void IgnoreProperty(Type type, params string[] propertyName)
        {
            ContractResolver.Ignore(type, propertyName);
        }
    }
}