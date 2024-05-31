namespace Azure.Data.Tables.EasyRepository;

public class TableConfiguration : ITableConfiguration
{
    public string TableName { get; }

    public TableConfiguration(string tableName)
    {
            TableName = tableName;
        }
}