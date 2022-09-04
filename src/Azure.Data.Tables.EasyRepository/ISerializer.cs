namespace Azure.Data.Tables.EasyRepository
{
    public interface ISerializer
    {
        string Serialize(object item);

        T Deserialize<T>(string value);
    }
}