using System;

namespace Azure.Data.Tables.EasyRepository;

public class InvalidComplexTypePropertyDeserialization<TTableEntity> : Exception
    where TTableEntity : class
{
    public InvalidComplexTypePropertyDeserialization(string propertyName) : base($"Property {propertyName} of type {typeof(TTableEntity).Name} is not nullable, but no values found in the table"){}
}