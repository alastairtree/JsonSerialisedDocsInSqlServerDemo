namespace PersistedDocDemo.Data
{
    public interface IRepositoryConfig
    {
        string ConnectionString { get; set; }
        string DatabaseSchemaName { get; set; }
    }
}