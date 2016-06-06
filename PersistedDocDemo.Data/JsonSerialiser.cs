using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace PersistedDocDemo.Data
{
    public class JsonSerialiser : IEntitySerialiser
    {
        private readonly List<Tuple<Type, string[]>> ignored;

        private readonly object lockObj = new object();

        public JsonSerialiser()
        {
            ignored = new List<Tuple<Type, string[]>>();
        }

        private JsonSerializerSettings SerialiserSettings { get; }

        public TEntity DeserializeObject<TEntity>(object data)
        {
            var serialiser = GetNewJsonSerializer();

            using (var reader = new JsonTextReader(new StringReader(data.ToString())))
            {
                return serialiser.Deserialize<TEntity>(reader);
            }
        }

        public object SerializeObject<TEntity>(TEntity item)
        {
            var serialiser = GetNewJsonSerializer();

            var sb = new StringBuilder(256);
            var sw = new StringWriter(sb, CultureInfo.InvariantCulture);
            using (var jsonWriter = new JsonTextWriter(sw))
            {
                jsonWriter.Formatting = serialiser.Formatting;

                serialiser.Serialize(jsonWriter, item);
            }

            return sb.ToString();
        }

        /// <summary>
        ///     Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyNames">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void IgnoreProperty(Type type, params string[] propertyNames)
        {
            ignored.Add(Tuple.Create(type, propertyNames));
        }

        internal object DeserializeObject(object data, Type type)
        {
            var serialiser = GetNewJsonSerializer();

            using (var reader = new JsonTextReader(new StringReader(data.ToString())))
            {
                return serialiser.Deserialize(reader, type);
            }
        }

        //we contruct our own jsonSerialiser because JsonConvert has some issues with the contract resolvers and concurrency meaning ignored properties get dropped
        private JsonSerializer GetNewJsonSerializer()
        {
            var resolver = new IgnorablePropertyCamelCaseNamesContractResolver
            {
                IgnoreSerializableAttribute = false
            };
            foreach (var item in ignored)
            {
                resolver.Ignore(item.Item1, item.Item2);
            }
            var serialiser = new JsonSerializer
            {
                ContractResolver = resolver
            };

            return serialiser;
        }
    }
}