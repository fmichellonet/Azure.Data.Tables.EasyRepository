using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository
{
    internal static class DictionaryExtensions
    {
        internal static IDictionary<string, object> StripComplexTypes<TEntity>(
            this IDictionary<string, object> source, IReadOnlyCollection<Expression<Func<TEntity, object>>> selectedProperties)
        {
            foreach (var customSerializedProperty in selectedProperties)
            {
                var prop = (PropertyInfo)((MemberExpression)customSerializedProperty.Body).Member;
                if (source.ContainsKey(prop.Name))
                {
                    source.Remove(prop.Name);
                }
            }

            return source;
        }

        internal static IDictionary<string, object> SerializeComplexType<TEntity>(
            this IDictionary<string, object> source,
            IReadOnlyCollection<Expression<Func<TEntity, object>>> selectedProperties, 
            TEntity item)
        {
            if (!selectedProperties.Any())
            {
                return source;
            }

            var serializer = new DefaultJsonSerializer();
            foreach (var property in selectedProperties)
            {
                var propertyInfo = (PropertyInfo)((MemberExpression)property.Body).Member;
                source.Add(propertyInfo.Name, serializer.Serialize(property.Compile()(item)));
            }

            return source;
        }

        internal static void DeserializeComplexType<TEntity>(
            this IDictionary<string, object> source,
            IReadOnlyCollection<Expression<Func<TEntity, object>>> selectedProperties,
            TEntity item)
        {
            if (!selectedProperties.Any())
            {
                return;
            }

            var serializer = new DefaultJsonSerializer();
            foreach (var property in selectedProperties)
            {
                var propertyInfo = (PropertyInfo)((MemberExpression)property.Body).Member;
                
                var newValue = Dynamic.InvokeMember(serializer,
                    new InvokeMemberName(nameof(serializer.Deserialize), propertyInfo.PropertyType),
                    source[propertyInfo.Name].ToString()
                );
                
                propertyInfo.SetValue(item, newValue);
            }
        }
    }
}