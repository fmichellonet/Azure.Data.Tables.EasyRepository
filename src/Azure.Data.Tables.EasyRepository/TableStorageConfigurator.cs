using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Data.Tables.EasyRepository;

public class DataTableConfiguration : IDataTableConfiguration
{
    private readonly IServiceCollection _services;
    private static readonly List<Type> ConfiguredRepositories = new List<Type>();

    public DataTableConfiguration(IServiceCollection services)
    {
        _services = services;
    }

    public IDataTableConfiguration AddRepositoryFor<TEntity>() where TEntity : class, ITableEntity, new()
    {
        ConfiguredRepositories.Add(typeof(ITableEntityRepository<TEntity>));

        _services.AddTransient<ITableEntityRepository<TEntity>>(sp =>
        {
            var repo = new TableEntityRepository<TEntity>(
                sp.GetRequiredService<TableServiceClient>(),
                new TableConfiguration(TableNameResolver.GetTableNameFor<TEntity>()));

            return repo;
        });

        return this;
    }

    public IDataTableConfiguration AddDynamicRepositoryFor<TEntity>(Func<TEntity, string> partitionKeySelector,
        Func<TEntity, string> rowKeySelector) where TEntity : class, new()
    {
        return AddDynamicRepositoryFor(
            new TableConfiguration(TableNameResolver.GetTableNameFor<TEntity>()),
            new TableEntityAdapter<TEntity>(partitionKeySelector, rowKeySelector));
    }

    public IDataTableConfiguration AddDynamicRepositoryFor<TEntity>(ITableConfiguration tableConfiguration, TableEntityAdapter<TEntity> tableAdapter)
        where TEntity : class, new()
    {
        ConfiguredRepositories.Add(typeof(IDynamicTableRepository<TEntity>));

        _services.AddTransient<IDynamicTableRepository<TEntity>>(sp =>
        {
            var repo = new DynamicTableRepository<TEntity>(
                sp.GetRequiredService<TableServiceClient>(),
                tableConfiguration,
                tableAdapter);

            return repo;
        });

        return this;
    }

    public static Task EnsureTablesExistAsync(IServiceProvider serviceProvider, CancellationToken cancellationToken = default)
    {
        return Task.WhenAll(ConfiguredRepositories.Select(x => ((TableRepositoryBase)serviceProvider.GetService(x)).CreateTableAsync(cancellationToken)));
    }
}