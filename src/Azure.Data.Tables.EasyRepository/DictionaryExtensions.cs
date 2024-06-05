using System.Collections.Generic;
using System.Linq;
using Azure.Data.Tables.EasyRepository.Serialization;

namespace Azure.Data.Tables.EasyRepository;

internal static class DictionaryExtensions
{
    internal static IDictionary<string, object> StripComplexTypes<TEntity>(
        this IDictionary<string, object> source,
        IReadOnlyCollection<IPropertySerializer<TEntity>> propertySerializationInformation,
        IReadOnlyCollection<IPropertyFlattener<TEntity>> propertyFlatteningInformation)
        where TEntity : class
    {
        foreach (var customSerializedProperty in propertySerializationInformation)
        {
            if (source.ContainsKey(customSerializedProperty.PropertyName))
            {
                source.Remove(customSerializedProperty.PropertyName);
            }
        }

        foreach (var customFlattenedProperty in propertyFlatteningInformation)
        {
            if (source.ContainsKey(customFlattenedProperty.PropertyName))
            {
                source.Remove(customFlattenedProperty.PropertyName);
            }
        }

        return source;
    }

    internal static IDictionary<string, object> SerializeComplexType<TEntity>(
        this IDictionary<string, object> source,
        IReadOnlyCollection<IPropertySerializer<TEntity>> propertySerializationInformations,
        TEntity item) where TEntity : class
    {
        if (!propertySerializationInformations.Any())
        {
            return source;
        }

        foreach (var property in propertySerializationInformations)
        {
            source.Add(property.PropertyName, property.SerializedValue(item));
        }

        return source;
    }

    internal static void DeserializeComplexType<TEntity>(
        this IDictionary<string, object> source,
        IReadOnlyCollection<IPropertySerializer<TEntity>> propertySerializationInformation,
        TEntity item) where TEntity : class
    {
        if (!propertySerializationInformation.Any())
        {
            return;
        }

        foreach (var serializationInformation in propertySerializationInformation)
        {
            if (source.TryGetValue(serializationInformation.PropertyName, out var value))
            {
                serializationInformation.SetValue(item, value.ToString());
                continue;
            }
            if (!serializationInformation.IsNullableProperty())
            {
                throw new InvalidComplexTypePropertyDeserialization<TEntity>(serializationInformation.PropertyName);
            }
        }
    }

    /// <summary>
    /// Adds complex type properties as new entries in the dictionary using
    /// property name and the computed prefix.
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <param name="source">The dictionary that represent the entity to store in Azure table</param>
    /// <param name="propertyFlatteningInformation"></param>
    /// <param name="item">The entity</param>
    /// <returns></returns>
    internal static IDictionary<string, object> FlattenComplexType<TEntity>(
        this IDictionary<string, object> source,
        IReadOnlyCollection<IPropertyFlattener<TEntity>> propertyFlatteningInformation,
        TEntity item) where TEntity : class
    {
        if (!propertyFlatteningInformation.Any())
        {
            return source;
        }

        foreach (var property in propertyFlatteningInformation)
        {
            source = source.Union(property.Flatten(item)).ToDictionary(x => x.Key, x => x.Value);
        }

        return source;
    }

    internal static void AggregateComplexType<TEntity>(
        this IDictionary<string, object> source,
        IReadOnlyCollection<IPropertyFlattener<TEntity>> propertyFlatteningInformation,
        TEntity item) where TEntity : class
    {
        if (!propertyFlatteningInformation.Any())
        {
            return;
        }

        foreach (var flatteningInformation in propertyFlatteningInformation)
        {
            flatteningInformation.AggregateAndSet(item,
                source.Where(x => x.Key.StartsWith(flatteningInformation.ColumnNamePrefix))
                    .ToDictionary(x => x.Key, x => x.Value));
        }
    }
}