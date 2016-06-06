using System;

namespace PersistedDocDemo.Data
{
    public interface IEntitySerialiser
    {
        TEntity DeserializeObject<TEntity>(object data);
        object SerializeObject<TEntity>(TEntity item);

        /// <summary>
        ///     Explicitly ignore the given property(s) for the given type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="propertyNames">one or more properties to ignore.  Leave empty to ignore the type entirely.</param>
        void IgnoreProperty(Type type, params string[] propertyNames);
    }
}