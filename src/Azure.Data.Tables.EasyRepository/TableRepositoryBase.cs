using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Data.Tables.EasyRepository
{
    public abstract class TableRepositoryBase<TTableEntity> : TableRepositoryBase
    {
        protected internal TableRepositoryBase(TableServiceClient tableServiceClient, string tableName) : base(tableServiceClient, tableName)
        {
        }

        protected Task ExecuteRange(IEnumerable<TTableEntity> items, TableTransactionActionType actionType,
            CancellationToken cancellationToken = default)
        {
            return Task.WhenAll(
                CreateTransactionGroups(items)
                    .Select(x => AsTransactionAction(x, actionType))
                    .Select(x => TableClient.SubmitTransactionAsync(x.Value, cancellationToken))
            );
        }

        protected abstract IReadOnlyCollection<IGrouping<string, TTableEntity>> CreateTransactionGroups(IEnumerable<TTableEntity> items);

        protected abstract KeyValuePair<string, TableTransactionAction[]> AsTransactionAction(IGrouping<string, TTableEntity> transaction, TableTransactionActionType actionType);
    }

    public abstract class TableRepositoryBase
    {
        public const int DefaultTransactionGroupSize = 100;
        protected readonly TableClient TableClient;
        
        protected internal TableRepositoryBase(TableServiceClient tableServiceClient, string tableName)
        {
            TableClient = tableServiceClient.GetTableClient(tableName);
        }
        
        public async Task TruncateAsync(CancellationToken cancellationToken = default)
        {
            await TableClient.DeleteAsync(cancellationToken);
            await CreateTableAsync(cancellationToken);
        }

        public Task CreateTableAsync(CancellationToken cancellationToken = default)
        {
            return TableClient.CreateIfNotExistsAsync(cancellationToken);
        }
        
        public Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        {
            return TableClient.DeleteEntityAsync(partitionKey, rowKey, ETag.All, cancellationToken);
        }

        public static IDynamicTableRepository<TTableEntity> For<TTableEntity>(TableServiceClient tableServiceClient,
            ITableConfiguration tableConfiguration,
            TableEntityAdapter<TTableEntity> tableEntityAdapter)
            where TTableEntity : class, new()
        {
            return new DynamicTableRepository<TTableEntity>(tableServiceClient, tableConfiguration, tableEntityAdapter);
        }

        public static ITableEntityRepository<TTableEntity> For<TTableEntity>(TableServiceClient tableServiceClient,
            ITableConfiguration tableConfiguration)
            where TTableEntity : class, ITableEntity, new()
        {
            return new TableEntityRepository<TTableEntity>(tableServiceClient, tableConfiguration);
        }
    }
}