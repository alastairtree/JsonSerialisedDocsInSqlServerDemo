# Sample APP to demo Json Serialised Docs In Sql Server
Sample app demonstrating storing serialised domain objects in sql server as json, using the repository pattern. Capable of storing large complex object graphs in one table.

## Using a generic repository interface for all your persistence

Using the interface below the app demonstrates persisting any complex object graph to a file or SQL Server in such a way that your domain model is seperated from the specifics of the persistence.

    public interface IRepository<T>
    {
        T Get(object id);
        ICollection<T> GetAll();
        void Save(T item);
        bool Delete(object id);
        bool Delete(T item);
        bool DeleteAll();
    }
