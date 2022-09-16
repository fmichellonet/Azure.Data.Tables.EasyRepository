using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Collections;
using Azure.Data.Tables.EasyRepository.Internals;

namespace Azure.Data.Tables.EasyRepository
{
    public class DynamicTableRepository<TTableEntity> : TableRepositoryBase<TTableEntity>, IDynamicTableRepository<TTableEntity>
        where TTableEntity : class, new()
    {
        private readonly TableEntityAdapter<TTableEntity> _tableEntityAdapter;
        private readonly TableClient _tableClient;

        public DynamicTableRepository(TableServiceClient tableServiceClient, 
            ITableConfiguration tableConfiguration,
            TableEntityAdapter<TTableEntity> tableEntityAdapter) : base(tableServiceClient, tableConfiguration.TableName)
        {
            _tableEntityAdapter = tableEntityAdapter;
            _tableClient = tableServiceClient.GetTableClient(tableConfiguration.TableName);
        }
        
        public async Task<TTableEntity> SingleAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        {
            var item = await SingleOrDefaultAsync(partitionKey, rowKey, cancellationToken);

            if (item is null)
            {
                throw new EntityNotFoundException<TTableEntity>(partitionKey, rowKey);
            }

            return item;
        }

        public async Task<TTableEntity?> SingleOrDefaultAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default)
        {
            var results = await WhereAsync(x => x.PartitionKey == partitionKey && x.RowKey == rowKey,
                cancellationToken: cancellationToken);

            return results.SingleOrDefault();
        }
        
        public Task<IReadOnlyList<TTableEntity>> WhereAsync(Expression<Func<TableEntityAdapter<TTableEntity>, bool>>? filter, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            var pagedQuery = _tableClient.Query(filter, pageSize, 
                serializationInformations: _tableEntityAdapter.Serializers,
                flatteningInformation: _tableEntityAdapter.Flatteners,
                cancellationToken: cancellationToken);

            var entities = new List<TTableEntity>();
            foreach (var page in pagedQuery.AsPages(pageSizeHint: pageSize))
            {
                entities.AddRange(page.Values);
            }

            IReadOnlyList<TTableEntity> result = entities.AsReadOnly();

            // till i've found out how to use QueryAsync
            return Task.FromResult(result);
        }
        
        public Task AddAsync(TTableEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            return TableClient.AddEntityAsync(_tableEntityAdapter.Wrap(item), cancellationToken);
        }
        
        public Task AddRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.Add, cancellationToken);
        }

        public Task DeleteRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.Delete, cancellationToken);
        }
        
        public Task<IReadOnlyList<TTableEntity>> ToListAsync(CancellationToken cancellationToken = default)
        {
            return WhereAsync(null, cancellationToken: cancellationToken);
        }

        public Task MergeAsync(TTableEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            return TableClient.UpdateEntityAsync(_tableEntityAdapter.Wrap(item), ETag.All, TableUpdateMode.Merge, cancellationToken);
        }

        public Task MergeRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.UpdateMerge, cancellationToken);
        }

        protected override IReadOnlyCollection<IGrouping<string, TTableEntity>> CreateTransactionGroups(IEnumerable<TTableEntity> items)
        {
            return items.GroupByBucket(x => _tableEntityAdapter.PartitionKeyExtractor(x), (pkey, incr) => $"{pkey}_{incr}", DefaultTransactionGroupSize)
                .ToArray();
        }

        protected override KeyValuePair<string, TableTransactionAction[]> AsTransactionAction(IGrouping<string, TTableEntity> transaction, TableTransactionActionType actionType)
        {
            return new KeyValuePair<string, TableTransactionAction[]>(transaction.Key,
                transaction.Select(x => new TableTransactionAction(actionType, _tableEntityAdapter.Wrap(x)))
                    .ToArray());
        }
    }
}