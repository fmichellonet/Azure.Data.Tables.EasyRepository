using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

public class DynamicMergeRangeAsyncShould
{
    private DynamicTableRepository<Car>? _repository;

    [OneTimeSetUp]
    public async Task OneTime()
    {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicMergeRangeAsyncShould));
            _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig, 
                new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model)); 
            await _repository.CreateTableAsync();
        }

    [SetUp]
    public async Task TearDown()
    {
            await _repository!.TruncateAsync();
            await _repository!.AddAsync(new Car { Brand = "Volvo", Model = "XC40", Color = "Black" });
            await _repository!.AddAsync(new Car { Brand = "Volvo", Model = "XC60", Color = "Black" });
            await _repository!.AddAsync(new Car { Brand = "Porsche", Model = "911", Color = "Black" });
        }

    [Test]
    public void Does_Not_Throws_When_Merging_Zero_Elements()
    {
            Assert.That(async () => await _repository!.MergeRangeAsync(new List<Car>()), Throws.Nothing);
        }


    [Test]
    public async Task Merge_One_Element()
    {
            await _repository!.MergeRangeAsync(new[]
            {
                new Car { Brand = "Volvo", Model = "XC40", Color = "Red" }
            });

            var retrievedCar = await _repository.SingleAsync("V", "XC40");

            Assert.Multiple(() =>
            {
                Assert.That(retrievedCar.Color == "Red");
            });
        }

    [Test]
    public async Task Merge_Multiple_Elements_On_Same_Partition()
    {
            Assert.That(async () => await _repository!.MergeRangeAsync(new[]
            {
                new Car{Brand = "Volvo", Model = "XC40", Color = "Red"},
                new Car{Brand = "Volvo", Model = "XC60", Color = "Red"}
            }), Throws.Nothing);

            var retrievedCars = await _repository!.WhereAsync(x => x.PartitionKey == "V");

            Assert.That(retrievedCars.All(x => x.Color == "Red"), Is.True);
        }

    [Test]
    public async Task Merge_Multiple_Elements_On_Different_Partitions()
    {
            Assert.That(async () => await _repository!.MergeRangeAsync(new[]
            {
                new Car{Brand = "Volvo", Model = "XC40", Color = "Red"},
                new Car { Brand = "Porsche", Model = "911", Color = "Red" }
            }), Throws.Nothing);

            var volvo = await _repository!.SingleAsync("V", "XC40");
            var porsche = await _repository!.SingleAsync("P", "911");

            Assert.That(volvo.Color, Is.EqualTo("Red"));
            Assert.That(porsche.Color, Is.EqualTo("Red"));
        }
}