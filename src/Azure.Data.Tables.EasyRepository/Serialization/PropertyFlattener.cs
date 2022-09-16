using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Dynamitey;

namespace Azure.Data.Tables.EasyRepository.Serialization
{
    internal class PropertyFlattener<TEntity, TProperty> : IPropertyFlattener<TEntity> 
        where TEntity : class where TProperty : new()

    {
        private readonly PropertyInfo _propertyInfo;

        private readonly IReadOnlyCollection<PropertyInfo> _subTypePropertiesInfo;

        public string ColumnNamePrefix => $"{PropertyName}_";

        public string PropertyName => _propertyInfo.Name;

        public PropertyFlattener(Expression<Func<TEntity, TProperty>> propertySelector)
        {
            _propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;

            _subTypePropertiesInfo = typeof(TProperty).GetProperties();
        }

        public IReadOnlyDictionary<string, object> Flatten(TEntity item)
        {
            var result = new Dictionary<string, object>();
            foreach (var propertyInfo in _subTypePropertiesInfo)
            {
                if (Dynamic.InvokeGet(item, PropertyName) == null)
                {
                    continue;
                }
                object value = Dynamic.InvokeGetChain(item, $"{PropertyName}.{propertyInfo.Name}");
                result.Add($"{ColumnNamePrefix}{propertyInfo.Name}", value);
            }

            return result;
        }

        public void AggregateAndSet(TEntity item, IReadOnlyDictionary<string, object> rawTableColumns)
        {
            var aggregated = new TProperty();

            foreach (var column in rawTableColumns)
            {
                Dynamic.InvokeSet(aggregated, column.Key[ColumnNamePrefix.Length..], column.Value);
            }

            Dynamic.InvokeSet(item, PropertyName, aggregated);
        }
    }
}