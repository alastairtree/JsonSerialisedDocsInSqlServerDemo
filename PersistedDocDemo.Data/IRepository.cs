using System.Collections.Generic;

namespace PersistedDocDemo.Data
{
    public interface IRepository<T>
    {
        T Get(object id);
        ICollection<T> GetAll();
        void Save(T item);
        bool Delete(object id);
        bool Delete(T item);
        bool DeleteAll();
    }
}