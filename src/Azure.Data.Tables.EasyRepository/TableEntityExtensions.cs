using System.Collections.Generic;
using Azure.Data.Tables.EasyRepository.Internals;

namespace Azure.Data.Tables.EasyRepository
{
    internal static class TableEntityExtensions
    {
        /// <summary>
        /// Returns a new Dictionary with the appropriate Odata type annotation for a given propertyName value pair.
        /// The default case is intentionally unhandled as this means that no type annotation for the specified type is required.
        /// This is because the type is naturally serialized in a way that the table service can interpret without hints.
        /// </summary>
        internal static IDictionary<string, object> ToDictionary<T>(this T entity)
        {
            if (entity is IDictionary<string, object> dictEntity)
            {
                return dictEntity.ToDictionary();
            }

            var dictionary = new Dictionary<string, object>();
            TablesTypeBinder
                .Shared()
                .GetBinderInfo<T>()
                .Serialize(entity, dictionary);

            return dictionary;
        }
    }
}