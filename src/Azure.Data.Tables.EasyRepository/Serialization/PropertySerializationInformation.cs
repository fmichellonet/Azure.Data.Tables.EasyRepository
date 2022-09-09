using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Azure.Data.Tables.EasyRepository.Serialization
{
    internal class PropertySerializationInformation<TEntity, TSerializer, TProperty> : IPropertySerializationInformation<TEntity>
        where TEntity : class
        where TSerializer : class, ISerializer
    {
        private readonly TSerializer _serializer;
        private readonly PropertyInfo _propertyInfo;
        private readonly Func<TEntity, TProperty> _propertyDelegate;

        public PropertySerializationInformation(TSerializer serializer,
            Expression<Func<TEntity, TProperty>> propertySelector)
        {
            _serializer = serializer;
            _propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
            _propertyDelegate = propertySelector.Compile();
        }

        public string PropertyName => _propertyInfo.Name;

        public string SerializedValue(TEntity item)
        {
            return _serializer.Serialize(_propertyDelegate(item));
        }

        public void SetValue(TEntity item, string value)
        {
            _propertyInfo.SetValue(item, _serializer.Deserialize<TProperty>(value));
        }
    }
}