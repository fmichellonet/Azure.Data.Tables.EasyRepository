namespace Azure.Data.Tables.EasyRepository.Serialization;

public interface ISerializer
{
    string Serialize<TProperty>(TProperty item);

    TProperty Deserialize<TProperty>(string value);
}