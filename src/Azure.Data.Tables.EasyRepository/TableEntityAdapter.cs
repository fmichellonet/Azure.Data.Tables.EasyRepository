using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
        
        private readonly List<Expression<Func<TEntity, object>>> _selectedProps;

        public IReadOnlyCollection<Expression<Func<TEntity, object>>> CustomPropertySerialization => _selectedProps;

        public TableEntityAdapter(Func<TEntity, string> partitionKey, Func<TEntity, string> rowKey)
        {
            PartitionKeyExtractor = partitionKey;
            RowKeyExtractor = rowKey;
            _selectedProps = new List<Expression<Func<TEntity, object>>>();
        }
        
        public TableEntityAdapter<TEntity> UseSerializerFor(Expression<Func<TEntity, object>> selector)
        {
            _selectedProps.Add(selector);
            return this;
        }

        public static IReadOnlyList<TEntity> ToEntityList(IReadOnlyCollection<IDictionary<string, object>> dictionary,
            IReadOnlyCollection<Expression<Func<TEntity, object>>> customPropertySerialization)
        {
            return dictionary.Select(x => ToEntity(x, customPropertySerialization)).ToArray();
        }

        public static TEntity ToEntity(IDictionary<string, object> dictionary, IReadOnlyCollection<Expression<Func<TEntity, object>>> customSerializedProperties)
        {
            var tablesTypeBinderTypeName = "Azure.Data.Tables.TablesTypeBinder";
            var deserializeMethodName = "Deserialize";
            var tablesTypeBinderType = typeof(TableEntity).Assembly.GetType(tablesTypeBinderTypeName);

            var binder = Dynamic.InvokeConstructor(tablesTypeBinderType);
            TEntity entity = Dynamic.InvokeMember(binder, new InvokeMemberName(deserializeMethodName, typeof(TEntity)), dictionary);

            dictionary.DeserializeComplexType(customSerializedProperties, entity);

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
                }.StripComplexTypes(_selectedProps)
                .SerializeComplexType(_selectedProps, item);

            return new TableEntity(dict);
        }
    }
}