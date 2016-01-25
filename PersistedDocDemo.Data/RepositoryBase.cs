using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace PersistedDocDemo.Data
{
    public abstract class RepositoryBase<T> : IRepository<T>
    {
        protected readonly string identityFieldName = "Id";
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

        protected object GetIdentityValue<TKey>(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var parameterExpression = Expression.Variable(typeof (T));
            var identityGetter = Expression.Property(parameterExpression, identityFieldName);
            var identityValueGetter = Expression.Lambda<Func<T, TKey>>(identityGetter, parameterExpression).Compile();
            return identityValueGetter.Invoke(item);
        }

        protected static void SetIdentity(T item, object id)
        {
            (item as Todo).Id = Convert.ToInt32(id);
        }
    }
}