using System;

namespace Azure.Data.Tables.EasyRepository;

public interface IDataTableConfiguration
{
    IDataTableConfiguration AddRepositoryFor<TEntity>() where TEntity : class, ITableEntity, new();

    IDataTableConfiguration AddDynamicRepositoryFor<TEntity>(Func<TEntity, string> partitionKeySelector,
        Func<TEntity, string> rowKeySelector) where TEntity : class, new();

    IDataTableConfiguration AddDynamicRepositoryFor<TEntity>(ITableConfiguration tableConfiguration, TableEntityAdapter<TEntity> tableAdapter)
        where TEntity : class, new();
}