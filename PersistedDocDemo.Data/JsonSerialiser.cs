using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PersistedDocDemo.Data
{
    public class JsonSerialiser : IEntitySerialiser
    {
        public JsonSerialiser()
        {
            JsonSerializerSettings = new JsonSerializerSettings();
            ContractResolver = new CamelCasePropertyNamesContractResolver {IgnoreSerializableAttribute = false};
            JsonSerializerSettings.ContractResolver = ContractResolver;
        }

        public CamelCasePropertyNamesContractResolver ContractResolver { get; set; }

        public JsonSerializerSettings JsonSerializerSettings { set; get; }

        public TEntity DeserializeObject<TEntity>(object data)
        {
            return JsonConvert.DeserializeObject<TEntity>(data.ToString(), JsonSerializerSettings);
        }

        public object SerializeObject<TEntity>(TEntity item)
        {
            return JsonConvert.SerializeObject(item, JsonSerializerSettings);
        }
    }
}