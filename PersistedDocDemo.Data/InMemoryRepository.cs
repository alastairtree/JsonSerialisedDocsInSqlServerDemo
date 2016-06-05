using System;
using System.Collections.Generic;
using System.Linq;

namespace PersistedDocDemo.Data
{
    public class InMemoryRepository<T> : RepositoryBase<T>
    {
        public InMemoryRepository(IEntitySerialiser serialiser, IRepositoryConfig config)
        {
            Serialiser = serialiser;
            Config = config;
            Store = new Dictionary<object, object>();
        }

        public InMemoryRepository() : this(new JsonSerialiser(), new DefaultRepositoryConfig())
        {
        }


        public IRepositoryConfig Config { get; }
        private Dictionary<object, object> Store { get; }

        public override T Get(object id)
        {
            var value = default(T);

            if (Store.ContainsKey(id))
            {
                var data = Store[id];

                if (data != null)
                {
                    value = Serialiser.DeserializeObject<T>(data);
                    SetIdentity(value, id);
                }
            }

            return value;
        }

        public override ICollection<T> GetAll()
        {
            var results = new List<T>();

            foreach (var data in Store.Values)
            {
                var item = Serialiser.DeserializeObject<T>(data);
                results.Add(item);
            }

            return results;
        }

        public override void Save(T item)
        {
            if (item == null) throw new ArgumentNullException("item");
            var id = GetIdentityValue(item);

            if (IsUndefinedKey(id))
            {
                throw new NotSupportedException("undefined key - you must use a key with a file repository");
            }

            var serialisedData = Serialiser.SerializeObject(item);

            Store[id] = serialisedData;
        }

        public override bool Delete(object id)
        {
            if (Store.ContainsKey(id))
            {
                Store.Remove(id);
                return true;
            }
            return false;
        }

        public override bool Delete(T item)
        {
            if (item == null) throw new ArgumentNullException("item");

            var id = GetIdentityValue(item);
            return Delete(id);
        }

        public override bool DeleteAll()
        {
            var results = 0;

            foreach (var id in Store.Keys.ToList())
            {
                if (Delete(id))
                    results++;
            }

            return results > 0;
        }
    }
}