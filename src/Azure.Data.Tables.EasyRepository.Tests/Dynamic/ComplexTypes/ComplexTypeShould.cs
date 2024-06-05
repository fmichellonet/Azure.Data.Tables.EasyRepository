using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Serialization;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.ComplexTypes;

public class ComplexTypeShould
{
    private DynamicTableRepository<Product>? _repository;

    [OneTimeSetUp]
    public void OneTime()
    {
        var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
        var tableConfig = new TableConfiguration(nameof(ComplexTypeShould));
        var adapter = new TableEntityAdapter<Product>(x => x.Name[..1], x => x.Name);
        adapter.UseSerializerFor(x => x.Weight);
        _repository = new DynamicTableRepository<Product>(serviceClient, tableConfig, adapter);
        _repository.CreateTableAsync();
    }

    [SetUp]
    public async Task TearDown()
    {
        await _repository!.TruncateAsync();
    }

    [Test]
    public async Task Support_One_Element_Round_Trip()
    {
        var product = new Product("Iron", new Weight(1500, Unit.g));
        await _repository!.AddAsync(product);

        var retrievedProduct = await _repository.SingleAsync("I", "Iron");

        Assert.That(retrievedProduct, Is.EqualTo(product));
    }

    [Test]
    public async Task Support_Multiple_Elements_Round_Trip()
    {
        var products = new[]
        {
            new Product("Iron", new Weight(1500, Unit.g)),
            new Product("Iced tea powder", new Weight(1, Unit.Kg))
        };
        await _repository!.AddRangeAsync(products);

        var retrievedProducts = await _repository.WhereAsync(x => x.PartitionKey == "I");

        Assert.That(products, Is.EquivalentTo(retrievedProducts.ToArray()));
    }

    [Test]
    public void Throw_On_Missing_Data_When_Deserializing_Non_Nullable_Property()
    {
        // Arrange
        var dict = new Dictionary<string, object>();
        var propSerializer =
            new PropertySerializer<Vegetable, DefaultJsonSerializer, List<string>>(new DefaultJsonSerializer(),
                x => x.Colors);

        // Act - Assert
        Assert.That(() => dict.DeserializeComplexType(new[] { propSerializer },
                new Vegetable()),
            Throws.InstanceOf<InvalidComplexTypePropertyDeserialization<Vegetable>>()
                .And.Message
                .EqualTo("Property Colors of type Vegetable is not nullable, but no values found in the table"));

    }
}