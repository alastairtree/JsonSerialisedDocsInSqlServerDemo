using System;
using System.Collections.Generic;

namespace PersistedDocDemo.Data
{
    public interface ISqlBuilder<TEntity>
    {
        string SelectByIdSql();
        string SelectAllSql();
        string SelectCountSql();
        string UpdateSql();
        string InsertSql();
        string DeleteByIdSql();
        string DeleteSql();
        void Init(IRepositoryConfig config, string identityFieldName, Dictionary<string, Type> columnMetadata);
    }
}