using System;

namespace Azure.Data.Tables.EasyRepository;

public class EntityNotFoundException<TTableEntity> : Exception
    where TTableEntity : class, new()
{
    public EntityNotFoundException(string partitionKey, string rowKey) : base($"Unable to find an entity of type {typeof(TTableEntity).Name} with partition key = {partitionKey} and row key = {rowKey}") { }
}