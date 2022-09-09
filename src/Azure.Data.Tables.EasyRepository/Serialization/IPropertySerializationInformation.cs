namespace Azure.Data.Tables.EasyRepository.Serialization
{
    public interface IPropertySerializationInformation<TEntity>
        where TEntity : class
    {
        string PropertyName { get; }
        string SerializedValue(TEntity item);
        void SetValue(TEntity item, string value);
    }
}