using System.Collections.Generic;

namespace Azure.Data.Tables.EasyRepository.Serialization;

public interface IPropertyFlattener<TEntity>
    where TEntity : class
{
    string PropertyName { get; }
    string ColumnNamePrefix { get; }
    IReadOnlyDictionary<string, object> Flatten(TEntity item);
    void AggregateAndSet(TEntity item, IReadOnlyDictionary<string, object> rawTableColumns);
}