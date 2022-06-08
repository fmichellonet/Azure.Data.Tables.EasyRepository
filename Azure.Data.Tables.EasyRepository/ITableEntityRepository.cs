using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Data.Tables.EasyRepository
{
    public interface ITableEntityRepository<TTableEntity> : ITableRepository<TTableEntity>
        where TTableEntity : class, ITableEntity, new()
    {
        Task<IReadOnlyList<TTableEntity>> WhereAsync(
            Expression<Func<TTableEntity, bool>>? filter, int? pageSize = null,
            CancellationToken cancellationToken = default);
    }
}