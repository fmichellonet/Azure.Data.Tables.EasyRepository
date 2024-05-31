using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace Azure.Data.Tables.EasyRepository;

public interface IDynamicTableRepository<TTableEntity> : ITableRepository<TTableEntity>
    where TTableEntity : class, new()
{
    Task<IReadOnlyList<TTableEntity>> WhereAsync(
        Expression<Func<TableEntityAdapter<TTableEntity>, bool>>? filter, int? pageSize = null,
        CancellationToken cancellationToken = default);
}