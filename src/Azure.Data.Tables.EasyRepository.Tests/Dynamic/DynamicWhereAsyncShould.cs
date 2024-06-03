using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

[TestFixture]
public class DynamicWhereAsyncShould
{

    private DynamicTableRepository<Car> _repository;

    [OneTimeSetUp]
    public async Task OneTimeSetUp()
    {
        var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
        var tableConfig = new TableConfiguration(nameof(DynamicWhereAsyncShould));
        _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig, new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model));
        await _repository.CreateTableAsync();
        await _repository!.AddRangeAsync(new[]
        {
                new Car() { Brand = "Volvo", Model = "XC40" },
                new Car() { Brand = "Volvo", Model = "XC60" },
                new Car() { Brand = "Fiat", Model = "Punto" }
            });
    }

    [OneTimeTearDown]
    public async Task OneTimeTearDown()
    {
        await _repository!.TruncateAsync();
    }

    [Test]
    public async Task Be_Queryable_Per_PartitionKey()
    {
        var res = await _repository.WhereAsync(x => x.PartitionKey == "V");

        Assert.That(res.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Be_Queryable_Per_RowKey()
    {
        var res = await _repository.WhereAsync(x => x.RowKey == "XC40");

        Assert.That(res.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Be_Queryable_Per_PartitionKey_And_RowKey()
    {
        var res = await _repository.WhereAsync(x => x.PartitionKey == "F" && x.RowKey == "Punto");

        Assert.That(res.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Be_Queryable_Per_Original_Property()
    {
        var res = await _repository.WhereAsync(x => x.OriginalEntity.Brand == "Volvo");

        Assert.That(res.Count, Is.EqualTo(2));
    }

    [Test]
    public async Task Returns_All_Pages()
    {
        var res = await _repository.WhereAsync(x => x.OriginalEntity.Brand == "Volvo", 1);

        Assert.That(res.Count, Is.EqualTo(2));
    }
}