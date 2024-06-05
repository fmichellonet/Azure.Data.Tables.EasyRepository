namespace Azure.Data.Tables.EasyRepository.Serialization;

public interface IPropertySerializer<TEntity>
    where TEntity : class
{
    string PropertyName { get; }
    string SerializedValue(TEntity item);
    void SetValue(TEntity item, string value);
    bool IsNullableProperty();
}