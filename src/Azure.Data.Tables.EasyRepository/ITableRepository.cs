using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Data.Tables.EasyRepository
{
    public interface ITableRepository<TTableEntity> 
        where TTableEntity : class, new()
    {
        Task<TTableEntity> SingleAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
        
        Task<TTableEntity?> SingleOrDefaultAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);
        
        Task<IReadOnlyList<TTableEntity>> ToListAsync(CancellationToken cancellationToken = default);

        Task AddAsync(TTableEntity item, CancellationToken cancellationToken = default);

        Task AddRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default);
        
        Task MergeAsync(TTableEntity item, CancellationToken cancellationToken = default);

        Task MergeRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default);

        Task DeleteAsync(string partitionKey, string rowKey, CancellationToken cancellationToken = default);

        Task DeleteRangeAsync(IEnumerable<TTableEntity> items, CancellationToken cancellationToken = default);

        Task TruncateAsync(CancellationToken cancellationToken = default);

        Task CreateTableAsync(CancellationToken cancellationToken = default);
        
    }
}