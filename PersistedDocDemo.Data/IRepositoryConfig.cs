using System.Security.Cryptography.X509Certificates;

namespace PersistedDocDemo.Data
{
    public interface IRepositoryConfig
    {
        string ConnectionString { get; set; }
        string DatabaseSchemaName { get; set; }

        string ColumnItemsDelimeter { get; set; }
    }
}