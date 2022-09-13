﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Azure.Data.Tables.EasyRepository.Serialization;
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
        
        private readonly List<IPropertySerializationInformation<TEntity>> _propertySerializationInformations;
        
        internal IReadOnlyCollection<IPropertySerializationInformation<TEntity>> SerializationInformations => _propertySerializationInformations.ToArray();

        private static readonly Dictionary<string, object> GlobalSerializationInformations =
            new Dictionary<string, object>();

        public TableEntityAdapter(Func<TEntity, string> partitionKey, Func<TEntity, string> rowKey)
        {
            PartitionKeyExtractor = partitionKey;
            RowKeyExtractor = rowKey;
            _propertySerializationInformations = new List<IPropertySerializationInformation<TEntity>>();
        }
        
        public TableEntityAdapter<TEntity> UseSerializerFor<TSerializer, TProperty>(Expression<Func<TEntity, TProperty>> selector)
            where TSerializer : class, ISerializer, new()
        {
            var selectorString = GetSelectorStringRepresentation<TSerializer, TProperty>(selector);
            if (GlobalSerializationInformations.ContainsKey(selectorString))
            {
                _propertySerializationInformations.Add((IPropertySerializationInformation<TEntity>)GlobalSerializationInformations[selectorString]);
                return this;
            }

            var serializationInfo = new PropertySerializationInformation<TEntity, TSerializer, TProperty>(new TSerializer(), selector);
            _propertySerializationInformations.Add(serializationInfo);
            GlobalSerializationInformations[selectorString] = serializationInfo;
            
            return this;
        }

        public TableEntityAdapter<TEntity> UseSerializerFor<TProperty>(Expression<Func<TEntity, TProperty>> selector)
        {
            return UseSerializerFor<DefaultJsonSerializer, TProperty>(selector);
        }
        
        internal static IReadOnlyList<TEntity> ToEntityList(IReadOnlyCollection<IDictionary<string, object>> dictionary,
            IReadOnlyCollection<IPropertySerializationInformation<TEntity>> serializationInformations)
        {
            return dictionary.Select(x => ToEntity(x, serializationInformations)).ToArray();
        }

        internal TableEntity Wrap(TEntity item)
        {
            var dict = new Dictionary<string, object>(ToDictionary(item))
                {
                    { nameof(PartitionKey), PartitionKeyExtractor(item) },
                    { nameof(RowKey), RowKeyExtractor(item) },
                    { nameof(Timestamp), DateTimeOffset.Now }
                }.StripComplexTypes(_propertySerializationInformations)
                .SerializeComplexType(_propertySerializationInformations, item);

            return new TableEntity(dict);
        }

        private static string GetSelectorStringRepresentation<TSerializer, TProperty>(
            Expression<Func<TEntity, TProperty>> selector)
        {
            return $@"<{typeof(TSerializer)}> ~> [{typeof(TEntity)}] : {selector}";
        }

        private static TEntity ToEntity(IDictionary<string, object> dictionary, IReadOnlyCollection<IPropertySerializationInformation<TEntity>> serializationInformations)
        {
            var tablesTypeBinderTypeName = "Azure.Data.Tables.TablesTypeBinder";
            var deserializeMethodName = "Deserialize";
            var tablesTypeBinderType = typeof(TableEntity).Assembly.GetType(tablesTypeBinderTypeName);

            var binder = Dynamic.InvokeConstructor(tablesTypeBinderType);
            TEntity entity = Dynamic.InvokeMember(binder, new InvokeMemberName(deserializeMethodName, typeof(TEntity)), dictionary);
            
            dictionary.DeserializeComplexType(serializationInformations, entity);

            return entity;
        }

        private static IDictionary<string, object> ToDictionary(TEntity entity)
        {
            var dictionaryTableExtensionsTypeName = "Azure.Data.Tables.TableEntityExtensions";
            var methodName = "ToOdataAnnotatedDictionary";

            var dictionaryTableExtensionsType =
                typeof(TableClient).Assembly.GetType(dictionaryTableExtensionsTypeName);

            return Dynamic.InvokeMember(InvokeContext.CreateStatic(dictionaryTableExtensionsType),
                new InvokeMemberName(methodName, typeof(TEntity)), entity);
        }
    }
}