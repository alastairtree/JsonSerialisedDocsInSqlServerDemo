using System.Configuration;

namespace PersistedDocDemo.Data
{
    internal class DefaultRepositoryConfig : IRepositoryConfig
    {
        public DefaultRepositoryConfig(string schemaName = "dbo", string connectionString = null)
        {
            DatabaseSchemaName = schemaName;
            ConnectionString = connectionString;

            TryAndSetADefaultConnectionString();
        }

        public string ConnectionString { get; set; }

        public string DatabaseSchemaName { get; set; }

        private void TryAndSetADefaultConnectionString()
        {
            if (string.IsNullOrEmpty(ConnectionString) && ConfigurationManager.ConnectionStrings.Count == 1)
            {
                ConnectionString = ConfigurationManager.ConnectionStrings[0].ConnectionString;
            }
        }
    }
}