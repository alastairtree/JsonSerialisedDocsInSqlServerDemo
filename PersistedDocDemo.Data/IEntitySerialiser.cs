namespace PersistedDocDemo.Data
{
    public interface IEntitySerialiser
    {
        TEntity DeserializeObject<TEntity>(object data);
        object SerializeObject<TEntity>(TEntity item);
    }
}