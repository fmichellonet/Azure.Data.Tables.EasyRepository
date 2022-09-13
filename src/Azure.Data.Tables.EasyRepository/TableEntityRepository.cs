using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Collections;

namespace Azure.Data.Tables.EasyRepository
{
    public class TableEntityRepository<TTableEntity> : TableRepositoryBase<TTableEntity>, ITableEntityRepository<TTableEntity>
        where TTableEntity : class, ITableEntity, new()
    {

        public TableEntityRepository(TableServiceClient tableServiceClient, ITableConfiguration tableConfiguration) : base(tableServiceClient, tableConfiguration.TableName) {}

        public async Task<IReadOnlyList<TTableEntity>> WhereAsync(Expression<Func<TTableEntity, bool>>? filter, int? pageSize = null, CancellationToken cancellationToken = default)
        {
            AsyncPageable<TTableEntity>? pagedQuery;
            if (filter is null)
            {
                pagedQuery = TableClient.QueryAsync<TTableEntity>((string)null, cancellationToken: cancellationToken);
            }
            else
            {
                pagedQuery = TableClient.QueryAsync(filter, cancellationToken: cancellationToken);
            }
            
            var entities = new List<TTableEntity>();
            await foreach (var page in pagedQuery.AsPages(pageSizeHint: pageSize).WithCancellation(cancellationToken))
            {
                entities.AddRange(page.Values);
            }

            return entities;
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
            try
            {
                var result = await TableClient.GetEntityAsync<TTableEntity>(partitionKey, rowKey, cancellationToken: cancellationToken);
                return result.Value;
            }
            catch (RequestFailedException rfe) when(rfe.Status == (int)HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public Task<IReadOnlyList<TTableEntity>> ToListAsync(CancellationToken cancellationToken = default)
        {
            return WhereAsync(null, cancellationToken: cancellationToken);
        }

        public Task AddRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.Add, cancellationToken);
        }

        public async Task AddAsync(TTableEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            await TableClient.AddEntityAsync(item, cancellationToken);
        }

        public Task MergeAsync(TTableEntity item, CancellationToken cancellationToken = default)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }
            return TableClient.UpdateEntityAsync(item, ETag.All, TableUpdateMode.Merge, cancellationToken);
        }

        public Task MergeRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.UpdateMerge, cancellationToken);
        }

        public Task DeleteRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default)
        {
            return ExecuteRange(items, TableTransactionActionType.Delete, cancellationToken);
        }

        protected override IReadOnlyCollection<IGrouping<string, TTableEntity>> CreateTransactionGroups(
            IEnumerable<TTableEntity> items)
        {
            return items.GroupByBucket(x => x.PartitionKey, (pkey, incr) => $"{pkey}_{incr}", DefaultTransactionGroupSize)
                .ToArray();
        }

        protected override KeyValuePair<string, TableTransactionAction[]> AsTransactionAction(IGrouping<string, TTableEntity> transaction, TableTransactionActionType actionType)
        {
            return new KeyValuePair<string, TableTransactionAction[]>(transaction.Key,
                transaction.Select(x => new TableTransactionAction(actionType, x))
                    .ToArray());
        }
    }
}