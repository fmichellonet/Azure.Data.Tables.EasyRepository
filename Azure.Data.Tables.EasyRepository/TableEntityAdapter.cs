using System;
using System.Collections.Generic;
using System.Linq;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository
{
    public class TableEntityAdapter<TEntity> : ITableEntity where TEntity : class
    {
        public string PartitionKey { get; set; }
     
        public string RowKey { get; set; }
        
        public DateTimeOffset? Timestamp { get; set; }
        
        public ETag ETag { get; set; }

        public TEntity OriginalEntity { get; }

        public readonly Func<TEntity, string> PartitionKeyExtractor;

        public readonly Func<TEntity, string> RowKeyExtractor;

        public TableEntityAdapter(Func<TEntity, string> partitionKey, Func<TEntity, string> rowKey)
        {
            PartitionKeyExtractor = partitionKey;
            RowKeyExtractor = rowKey;
        }
        
        public static IReadOnlyList<TEntity> ToEntityList(IReadOnlyCollection<IDictionary<string, object>> dictionary)
        {
            return dictionary.Select(ToEntity).ToArray();
        }

        public static TEntity ToEntity(IDictionary<string, object> dictionary)
        {
            var tablesTypeBinderTypeName = "Azure.Data.Tables.TablesTypeBinder";
            var deserializeMethodName = "Deserialize";
            var tablesTypeBinderType = typeof(TableEntity).Assembly.GetType(tablesTypeBinderTypeName);

            var binder = Dynamic.InvokeConstructor(tablesTypeBinderType);
            TEntity entity = Dynamic.InvokeMember(binder, new InvokeMemberName(deserializeMethodName, typeof(TEntity)), dictionary);
            
            return entity;
        }

        public static IDictionary<string, object> ToDictionary(TEntity entity)
        {
            var dictionaryTableExtensionsTypeName = "Azure.Data.Tables.TableEntityExtensions";
            var methodName = "ToOdataAnnotatedDictionary";

            var dictionaryTableExtensionsType =
                typeof(TableClient).Assembly.GetType(dictionaryTableExtensionsTypeName);

            return Dynamic.InvokeMember(InvokeContext.CreateStatic(dictionaryTableExtensionsType),
                new InvokeMemberName(methodName, typeof(TEntity)), entity);
        }

        public TableEntity Wrap(TEntity item)
        {
            var dict = new Dictionary<string, object>(ToDictionary(item))
            {
                { nameof(PartitionKey), PartitionKeyExtractor(item) },
                { nameof(RowKey), RowKeyExtractor(item) },
                { nameof(Timestamp), DateTimeOffset.Now }
            };

            return new TableEntity(dict);
        }
    }
}