using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace PersistedDocDemo.Data
{
    /// <summary>
    ///     Special JsonConvert resolver that allows you to ignore properties.  See http://stackoverflow.com/a/13588192/1037948
    /// </summary>
    public class IgnorablePropertyCamelCaseNamesContractResolver : CamelCasePropertyNamesContractResolver
    {
        protected readonly Dictionary<Type, HashSet<string>> Ignores;

        public IgnorablePropertyCamelCaseNamesContractResolver()
        {
            Ignores = new Dictionary<Type, HashSet<string>>();
        }

        /// <summary>
        ///     Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        public void Ignore(Type type, params string[] propertyName)
        {
            // start bucket if DNE
            if (!Ignores.ContainsKey(type)) Ignores[type] = new HashSet<string>();

            foreach (var prop in propertyName)
            {
                var camelCasedPropertyName = ResolvePropertyName(prop);
                Ignores[type].Add(camelCasedPropertyName);
            }
        }

        /// <summary>
        ///     Is the given property for the given type ignored?
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyName"></param>
        /// <returns></returns>
        public bool IsIgnored(Type type, string propertyName)
        {
            if (!Ignores.ContainsKey(type)) return false;

            // if no properties provided, ignore the type entirely
            if (Ignores[type].Count == 0) return true;

            var camelCasedPropertyName = ResolvePropertyName(propertyName);


            return Ignores[type].Contains(camelCasedPropertyName);
        }

        /// <summary>
        ///     The decision logic goes here
        /// </summary>
        /// <param name="member"></param>
        /// <param name="memberSerialization"></param>
        /// <returns></returns>
        protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            if (IsIgnored(property.DeclaringType, property.PropertyName)
                // need to check basetype as well for EF -- @per comment by user576838
                || IsIgnored(property.DeclaringType.BaseType, property.PropertyName))
            {
                property.ShouldSerialize = instance => { return false; };
            }

            return property;
        }
    }
}