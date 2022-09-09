using System.Collections.Generic;
using System.Linq;
using Azure.Data.Tables.EasyRepository.Serialization;

namespace Azure.Data.Tables.EasyRepository
{
    internal static class DictionaryExtensions
    {
        internal static IDictionary<string, object> StripComplexTypes<TEntity>(
            this IDictionary<string, object> source, IReadOnlyCollection<IPropertySerializationInformation<TEntity>> propertySerializationInformations) 
            where TEntity : class
        {
            foreach (var customSerializedProperty in propertySerializationInformations) 
            {
                if (source.ContainsKey(customSerializedProperty.PropertyName))
                {
                    source.Remove(customSerializedProperty.PropertyName);
                }
            }

            return source;
        }

        internal static IDictionary<string, object> SerializeComplexType<TEntity>(
            this IDictionary<string, object> source,
            IReadOnlyCollection<IPropertySerializationInformation<TEntity>> propertySerializationInformations,
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
            IReadOnlyCollection<IPropertySerializationInformation<TEntity>> propertySerializationInformations,
            TEntity item) where TEntity : class
        {
            if (!propertySerializationInformations.Any())
            {
                return;
            }
            
            foreach (var property in propertySerializationInformations)
            {
                property.SetValue(item, source[property.PropertyName].ToString());
            }
        }
    }
}