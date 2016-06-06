using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;

namespace PersistedDocDemo.Data
{
    public class JsonSerialiser : IEntitySerialiser
    {
        private List<Tuple<Type, string[]>> ignored;

        public JsonSerialiser()
        {
            this.ignored = new List<Tuple<Type, string[]>>();
        }

        IgnorablePropertyCamelCaseNamesContractResolver ContractResolver { get; }

        JsonSerializerSettings SerialiserSettings { get; }

        public TEntity DeserializeObject<TEntity>(object data)
        {
            var serialiser = GetNewJsonSerializer();

            using (var reader = new JsonTextReader(new StringReader(data.ToString())))
            {
                return serialiser.Deserialize<TEntity>(reader);
            }

            return JsonConvert.DeserializeObject<TEntity>(data.ToString(), SerialiserSettings);
        }

        internal object DeserializeObject(object data, Type type)
        {
            var serialiser = GetNewJsonSerializer();

            using (var reader = new JsonTextReader(new StringReader(data.ToString())))
            {
                return serialiser.Deserialize(reader, type);
            }


            return JsonConvert.DeserializeObject(data.ToString(), type, SerialiserSettings);
        }

        object lockObj = new object();
        public object SerializeObject<TEntity>(TEntity item)
        {
            lock (lockObj)
            {
                var serialiser = GetNewJsonSerializer();


                StringBuilder sb = new StringBuilder(256);
                StringWriter sw = new StringWriter(sb, CultureInfo.InvariantCulture);
                using (JsonTextWriter jsonWriter = new JsonTextWriter(sw))
                {
                    jsonWriter.Formatting = serialiser.Formatting;

                    serialiser.Serialize(jsonWriter, item);
                }

                return sb.ToString();
            }


            return JsonConvert.SerializeObject(item, SerialiserSettings);
        }

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
            var serialiser = new Newtonsoft.Json.JsonSerializer
            {
                ContractResolver = resolver
            };

            return serialiser;
        }

        /// <summary>
        ///     Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyNames">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void IgnoreProperty(Type type, params string[] propertyNames)
        {
            Debug.WriteLine("jsonserialiser ignoring " + type.Name + "" + string.Join(" ", propertyNames)); 
           ignored.Add(Tuple.Create(type, propertyNames));
        }
    }
}