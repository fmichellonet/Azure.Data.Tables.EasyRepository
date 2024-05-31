using System;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

public class DynamicAddAsyncShould
{
    private DynamicTableRepository<Car>? _repository;

    [OneTimeSetUp]
    public void OneTime()
    {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicAddAsyncShould));
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
    public void Throws_ArgumentNullException_When_Passing_Null()
    {
            Assert.That(async () => await _repository!.AddAsync(null), Throws.InstanceOf<ArgumentNullException>());
        }

    [Test]
    public async Task Add_One_Element()
    {
            await _repository!.AddAsync(new Car { Brand = "Volvo", Model = "XC40" });

            var retrievedCar = await _repository.SingleAsync("V", "XC40");

            Assert.Multiple(() =>
            {
                Assert.That(retrievedCar.Brand == "Volvo");
                Assert.That(retrievedCar.Model == "XC40");
            });
        }
}