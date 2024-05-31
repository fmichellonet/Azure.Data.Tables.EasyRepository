using System;

namespace Azure.Data.Tables.EasyRepository;

public static class TableNameResolver
{
    public static string GetTableNameFor<TTableEntity>()
        where TTableEntity : new()
    {
            var tableAttribute = (TableAttribute?)Attribute.GetCustomAttribute(typeof(TTableEntity), typeof(TableAttribute));

            return tableAttribute == null ? typeof(TTableEntity).Name : tableAttribute.Name;
        }
}