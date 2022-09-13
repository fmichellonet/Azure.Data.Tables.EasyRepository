using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;

namespace Azure.Data.Tables.EasyRepository
{
    public class DataTableConfiguration : IDataTableConfiguration
    {
        private readonly IServiceCollection _services;
        private readonly List<TableRepositoryBase> _configuredRepositories = new List<TableRepositoryBase>();

        public DataTableConfiguration(IServiceCollection services)
        {
            _services = services;
        }
        
        public IDataTableConfiguration AddRepositoryFor<TEntity>() where TEntity : class, ITableEntity, new()
        {
            _services.AddTransient<ITableEntityRepository<TEntity>>(sp =>
            {
                var repo = new TableEntityRepository<TEntity>(
                    sp.GetRequiredService<TableServiceClient>(),
                    new TableConfiguration(TableNameResolver.GetTableNameFor<TEntity>()));

                _configuredRepositories.Add(repo);

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
            _services.AddTransient<IDynamicTableRepository<TEntity>>(sp =>
            {
                var repo = new DynamicTableRepository<TEntity>(
                            sp.GetRequiredService<TableServiceClient>(),
                            tableConfiguration,
                            tableAdapter);

                _configuredRepositories.Add(repo);

                return repo;
            });

            return this;
        }
        
        public Task EnsureTablesExistAsync(CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(_configuredRepositories.Select(x => x.CreateTableAsync(cancellationToken)));
        }
    }
}