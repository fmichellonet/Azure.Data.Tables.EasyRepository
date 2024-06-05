using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Azure.Data.Tables.EasyRepository.Serialization;

internal class PropertySerializer<TEntity, TSerializer, TProperty> : IPropertySerializer<TEntity>
    where TEntity : class
    where TSerializer : class, ISerializer
{
    private readonly TSerializer _serializer;
    private readonly PropertyInfo _propertyInfo;
    private readonly Func<TEntity, TProperty> _propertyDelegate;

    public PropertySerializer(TSerializer serializer,
        Expression<Func<TEntity, TProperty>> propertySelector)
    {
        _serializer = serializer;
        _propertyInfo = (PropertyInfo)((MemberExpression)propertySelector.Body).Member;
        _propertyDelegate = propertySelector.Compile();
    }

    public string PropertyName => _propertyInfo.Name;

    public bool IsNullableProperty()
    {
        
        if (Nullable.GetUnderlyingType(_propertyInfo.PropertyType) != null)
        {
            return true; // Nullable<T>
        }

        // Check for Nullable attribute on the property
        var nullableAttribute = _propertyInfo.CustomAttributes
            .FirstOrDefault(attr => attr.AttributeType.FullName == "System.Runtime.CompilerServices.NullableAttribute");
        var isNullable = nullableAttribute is { ConstructorArguments.Count: > 0 } && nullableAttribute.ConstructorArguments[0].ArgumentType == typeof(byte[]);

        return isNullable;
    }

    public string SerializedValue(TEntity item)
    {
        return _serializer.Serialize(_propertyDelegate(item));
    }

    public void SetValue(TEntity item, string value)
    {
        _propertyInfo.SetValue(item, _serializer.Deserialize<TProperty>(value));
    }
}