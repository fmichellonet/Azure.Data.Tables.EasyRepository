using System.Text.Json;

namespace Azure.Data.Tables.EasyRepository.Serialization
{
    public class DefaultJsonSerializer : ISerializer
    {
        public string Serialize<TProperty>(TProperty item)
        {
            return JsonSerializer.Serialize(item);
        }

        public TProperty Deserialize<TProperty>(string value)
        {
            return JsonSerializer.Deserialize<TProperty>(value);
        }
    }
}