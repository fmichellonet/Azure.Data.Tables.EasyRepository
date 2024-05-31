using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Azure.Data.Tables.EasyRepository.Internals;
using Azure.Data.Tables.EasyRepository.Serialization;

[assembly: InternalsVisibleTo("Azure.Data.Tables.EasyRepository.Tests")]
namespace Azure.Data.Tables.EasyRepository
{
    public abstract class TableEntityAdapterBase
    {
        protected static readonly Dictionary<string, object> GlobalSerializers = new Dictionary<string, object>();

        protected static readonly Dictionary<string, object> GlobalFlatteners = new Dictionary<string, object>();
    }

    public class TableEntityAdapter<TEntity> : TableEntityAdapterBase, ITableEntity where TEntity : class
    {
        public string PartitionKey { get; set; }

        public string RowKey { get; set; }

        public DateTimeOffset? Timestamp { get; set; }

        public ETag ETag { get; set; }

        public TEntity OriginalEntity { get; }

        public readonly Func<TEntity, string> PartitionKeyExtractor;

        public readonly Func<TEntity, string> RowKeyExtractor;

        private readonly List<IPropertySerializer<TEntity>> _propertySerializers;

        private readonly List<IPropertyFlattener<TEntity>> _propertyFlatteners;

        internal IReadOnlyCollection<IPropertySerializer<TEntity>> Serializers => _propertySerializers.ToArray();

        internal IReadOnlyCollection<IPropertyFlattener<TEntity>> Flatteners => _propertyFlatteners.ToArray();
        
        public TableEntityAdapter(Func<TEntity, string> partitionKey, Func<TEntity, string> rowKey)
        {
            PartitionKeyExtractor = partitionKey;
            RowKeyExtractor = rowKey;
            _propertySerializers = new List<IPropertySerializer<TEntity>>();
            _propertyFlatteners = new List<IPropertyFlattener<TEntity>>();
        }

        public TableEntityAdapter<TEntity> UseSerializerFor<TSerializer, TProperty>(Expression<Func<TEntity, TProperty>> selector)
            where TSerializer : class, ISerializer, new()
        {
            var selectorString = GetSelectorStringRepresentation<TSerializer, TProperty>(selector);
            if (GlobalSerializers.TryGetValue(selectorString, out var propertySerializer))
            {
                _propertySerializers.Add((IPropertySerializer<TEntity>)propertySerializer);
                return this;
            }

            var serializationInfo = new PropertySerializer<TEntity, TSerializer, TProperty>(new TSerializer(), selector);
            _propertySerializers.Add(serializationInfo);
            GlobalSerializers[selectorString] = serializationInfo;

            return this;
        }

        public TableEntityAdapter<TEntity> UseSerializerFor<TProperty>(Expression<Func<TEntity, TProperty>> selector)
        {
            return UseSerializerFor<DefaultJsonSerializer, TProperty>(selector);
        }

        public TableEntityAdapter<TEntity> Flatten<TProperty>(Expression<Func<TEntity, TProperty>> selector) where TProperty : new()
        {
            var selectorString = GetSelectorStringRepresentation(selector);

            if (GlobalFlatteners.TryGetValue(selectorString, out var flattener))
            {
                _propertyFlatteners.Add((IPropertyFlattener<TEntity>)flattener);

                return this;
            }

            var flatteningInfo = new PropertyFlattener<TEntity, TProperty>(selector);
            _propertyFlatteners.Add(flatteningInfo);
            GlobalFlatteners[selectorString] = flatteningInfo;

            return this;
        }

        internal static IReadOnlyList<TEntity> ToEntityList(IReadOnlyCollection<IDictionary<string, object>> dictionary,
            IReadOnlyCollection<IPropertySerializer<TEntity>> serializationInformation,
            IReadOnlyCollection<IPropertyFlattener<TEntity>> flatteningInformation)
        {
            return dictionary.Select(x => ToEntity(x, serializationInformation, flatteningInformation)).ToArray();
        }

        internal TableEntity Wrap(TEntity item)
        {
            var dict = new Dictionary<string, object>(item.ToDictionary())
                {
                    { nameof(PartitionKey), PartitionKeyExtractor(item) },
                    { nameof(RowKey), RowKeyExtractor(item) },
                    { nameof(Timestamp), DateTimeOffset.Now }
                }.StripComplexTypes(_propertySerializers, _propertyFlatteners)
                .SerializeComplexType(_propertySerializers, item)
                .FlattenComplexType(_propertyFlatteners, item);

            return new TableEntity(dict);
        }

        private static string GetSelectorStringRepresentation<TProperty>(
            Expression<Func<TEntity, TProperty>> selector)
        {
            return $"[{typeof(TEntity)}] : {selector}";
        }

        private static string GetSelectorStringRepresentation<TSerializer, TProperty>(
            Expression<Func<TEntity, TProperty>> selector)
        {
            return $"<{typeof(TSerializer)}> ~> [{typeof(TEntity)}] : {selector}";
        }

        private static TEntity ToEntity(IDictionary<string, object> dictionary,
            IReadOnlyCollection<IPropertySerializer<TEntity>> serializationInformation,
            IReadOnlyCollection<IPropertyFlattener<TEntity>> flatteningInformation)
        {
            var entity = TablesTypeBinder
                .Shared()
                .GetBinderInfo<TEntity>()
                .Deserialize(dictionary);

            dictionary.DeserializeComplexType(serializationInformation, entity);

            dictionary.AggregateComplexType(flatteningInformation, entity);

            return entity;
        }
    }
}