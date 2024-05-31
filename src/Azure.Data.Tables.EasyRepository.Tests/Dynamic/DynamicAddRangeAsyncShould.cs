using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

public class DynamicAddRangeAsyncShould
{
    private DynamicTableRepository<Car>? _repository;

    [OneTimeSetUp]
    public void OneTime()
    {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicAddRangeAsyncShould));
            _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig, 
                new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model));
            _repository.CreateTableAsync();
        }

    [SetUp]
    public async Task TearDown()
    {
            await _repository!.TruncateAsync();
        }

    [Test]
    public void Does_Not_Throws_When_Adding_Zero_Elements()
    {
            Assert.That(async () => await _repository!.AddRangeAsync(new List<Car>()), Throws.Nothing);
        }

    [Test]
    public void Add_One_Element()
    {
            Assert.That(async () => await _repository!.AddRangeAsync(new []
            {
                new Car{Brand = "Volvo", Model = "XC40"}
            }), Throws.Nothing);
        }

    [Test]
    public void Add_Multiple_Elements_On_Same_Partition()
    {
            Assert.That(async () => await _repository!.AddRangeAsync(new[]
            {
                new Car{Brand = "Volvo", Model = "XC40"},
                new Car{Brand = "Volvo", Model = "XC60"}
            }), Throws.Nothing);
        }

    [Test]
    public void Add_Multiple_Elements_On_Different_Partitions()
    {
            Assert.That(async () => await _repository!.AddRangeAsync(new[]
            {
                new Car{Brand = "Volvo", Model = "XC40"},
                new Car{Brand = "Volvo", Model = "XC60"},
                new Car{Brand = "Fiat", Model = "Punto"},
            }), Throws.Nothing);
        }
}