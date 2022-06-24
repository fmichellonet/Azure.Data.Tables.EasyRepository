using System;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models
{
    public class Product : ITableEntity
    {
        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public double Weight { get; set; }
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public Product(){}

        public Product(string partitionKey, string rowKey)
        {
            PartitionKey = partitionKey;
            RowKey = rowKey;
            Timestamp = DateTimeOffset.Now;
            ETag = new ETag("*");
        }
    }
}