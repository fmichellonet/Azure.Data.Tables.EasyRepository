using System;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

public class DynamicMergeAsyncShould
{
    private DynamicTableRepository<Car>? _repository;

    [OneTimeSetUp]
    public async Task OneTime()
    {
        var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
        var tableConfig = new TableConfiguration(nameof(DynamicMergeAsyncShould));
        _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig,
            new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model));
        await _repository.CreateTableAsync();
    }

    [SetUp]
    public async Task SetUp()
    {
        await _repository!.TruncateAsync();
        await _repository!.AddAsync(new Car { Brand = "Volvo", Model = "XC40", Color = "Black" });
    }

    [Test]
    public void Throws_ArgumentNullException_When_Passing_Null()
    {
        Assert.That(async () => await _repository!.MergeAsync(null), Throws.InstanceOf<ArgumentNullException>());
    }

    [Test]
    public async Task Merge_One_Element()
    {
        await _repository!.MergeAsync(new Car { Brand = "Volvo", Model = "XC40", Color = "Red" });

        var retrievedCar = await _repository.SingleAsync("V", "XC40");

        Assert.Multiple(() =>
        {
            Assert.That(retrievedCar.Color == "Red");
        });
    }
}