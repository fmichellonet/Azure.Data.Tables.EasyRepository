namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;

public record Product(string Name, Weight Weight)
{
    public Product() : this(string.Empty, new Weight(0, Unit.g))
    {
    }
};