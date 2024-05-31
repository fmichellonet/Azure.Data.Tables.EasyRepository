using System;
using System.Collections.Generic;
using Dynamitey;

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
            TablesTypeBinder.Shared().GetBinderInfo(typeof(T)).Serialize(entity, dictionary);

            return dictionary;
        }

        private class TablesTypeBinder
        {
            private readonly dynamic _surrogateType;

            private static TablesTypeBinder? _instance;

            private TablesTypeBinder(dynamic surrogateType)
            {
                _surrogateType = surrogateType;
            }

            public static TablesTypeBinder Shared()
            {
                if (_instance != null)
                {
                    return _instance;
                }
                var tablesTypeBinderTypeName = "Azure.Data.Tables.TablesTypeBinder";
                var tablesTypeBinderType = typeof(TableClient).Assembly.GetType(tablesTypeBinderTypeName);
                var propertyBackingAccessorName = "get_Shared";

                var surrogate = Dynamic.InvokeMember(InvokeContext.CreateStatic(tablesTypeBinderType), propertyBackingAccessorName);
                _instance = new TablesTypeBinder(surrogate);
                return _instance;
            }

            public BoundTypeInfo GetBinderInfo(Type entityType)
            {
                var typeInfo = Dynamic.InvokeMember(_surrogateType, "GetBinderInfo", new object[] { entityType });
                return new BoundTypeInfo(typeInfo);
            }
        }

        private class BoundTypeInfo
        {
            private readonly dynamic _surrogateType;

            public BoundTypeInfo(dynamic surrogateType)
            {
                _surrogateType = surrogateType;
            }

            public void Serialize<T>(T entity, Dictionary<string, object> dictionary)
            {
                Dynamic.InvokeMemberAction(_surrogateType, new InvokeMemberName(nameof(Serialize), typeof(T)), new object[] { entity, dictionary });
            }
        }
    }
}