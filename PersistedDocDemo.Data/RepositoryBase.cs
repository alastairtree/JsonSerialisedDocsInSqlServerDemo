using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace PersistedDocDemo.Data
{
    public abstract class RepositoryBase<T> : IRepository<T>
    {
        protected RepositoryBase()
        {
            // TODO: Add caching on reflection to get identity
            IdentityFieldName = GetIdentityFieldName();
        }

        public string IdentityFieldName { get; }
        public IEntitySerialiser Serialiser { get; protected set; }
        public abstract T Get(object id);
        public abstract ICollection<T> GetAll();
        public abstract void Save(T item);
        public abstract bool Delete(object id);
        public abstract bool Delete(T item);
        public abstract bool DeleteAll();

        protected bool IsUndefinedKey(object id)
        {
            return id == null || id.Equals(0) || id.Equals(string.Empty) || id.Equals(Guid.Empty);
        }

        protected object GetIdentityValue(T item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (string.IsNullOrEmpty(IdentityFieldName))
                throw new NotSupportedException(
                    "Unable to determine identity field by convention - add a [Key] attribute or Id property");

            return GetValueFromProperty(item, IdentityFieldName);
        }

        protected static object GetValueFromProperty(T item, string propertyName)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            if (string.IsNullOrEmpty(propertyName)) throw new NotSupportedException(
                    "Unable to determine which property to read from");

            var identityValueGetter = CreateLambdaGetter(propertyName);

            return identityValueGetter.Invoke(item);
        }

        private static Func<T, object> CreateLambdaGetter(string fieldName)
        {
            var itemExpressionVariable = Expression.Variable(typeof(T));
            var propertyExpression = Expression.Property(itemExpressionVariable, fieldName);

            Expression propertyExpressionToObject = Expression.Convert(propertyExpression, typeof(object));
            var getter =
                Expression.Lambda<Func<T, object>>(propertyExpressionToObject, itemExpressionVariable).Compile();

            return getter;
        }

        protected void SetIdentity(T item, object id)
        {
            SetProperty(item, IdentityFieldName, id);
        }

        protected void SetProperty(T item, string propertyName, object value)
        {
            if (value != DBNull.Value)
            {
               var  setter = CreatePropertySetter(propertyName, value.GetType());
                setter(item, value);
            }
        }

        protected static Action<T, object> CreatePropertySetter(string propertyName, Type valueType)
        {
            var target = Expression.Parameter(typeof(T), "obj");
            var value = Expression.Parameter(typeof(object), "value`");
            var property = typeof(T).GetProperty(propertyName);
            var unboxed = Expression.Convert(value, valueType);
            var body = Expression.Assign(
                Expression.Property(target, property),
                Expression.Convert(unboxed, property.PropertyType));

            var lambda = Expression.Lambda<Action<T, object>>(body, target, value);
            return lambda.Compile();
        }

        protected string GetIdentityFieldName()
        {
            //try and get by [Key] attributePredicate
            var byAttribute = GetPropertyNameByCustomAttribute<T, KeyAttribute>();
            if (byAttribute.Any())
            {
                if (byAttribute.Length > 1)
                    throw new NotSupportedException(
                        "Cannot support compound keys - entities with more then one property decorated with a [Key] attribute are not allowed. Use an integer Identity instead?");
                return byAttribute.Single();
            }

            //try and get by naming convention [Id, EntityId, Key, EntityKey]
            var typeName = typeof(T).Name.ToLower();
            foreach (
                var property in
                    typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public).Select(p => p.Name))
            {
                if (property.ToLower() == "id") return property;
                if (property.ToLower() == (typeName + "id")) return property;
                if (property.ToLower() == "key") return property;
                if (property.ToLower() == (typeName + "key")) return property;
            }
            return null;
        }

        protected static string[] GetPropertyNameByCustomAttribute<ClassToAnalyse, AttributeTypeToFind>()
            where AttributeTypeToFind : Attribute
        {
            return (from propertyInfo in
                typeof(ClassToAnalyse).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                    where propertyInfo.GetCustomAttributes(typeof(AttributeTypeToFind), true).Any()
                    select propertyInfo.Name)
                .ToArray();
        }
    }
}