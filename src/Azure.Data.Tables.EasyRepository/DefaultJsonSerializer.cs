using System.Text.Json;

namespace Azure.Data.Tables.EasyRepository
{
    public class DefaultJsonSerializer : ISerializer
    {
        public string Serialize(object item)
        {
            return JsonSerializer.Serialize(item);
        }

        public T Deserialize<T>(string value)
        {
            return JsonSerializer.Deserialize<T>(value);
        }
    }
}