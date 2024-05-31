using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic;

[TestFixture]
public class DynamicToListAsyncShould
{

    private DynamicTableRepository<Car> _repository;

    [OneTimeSetUp]
    public void OneTime()
    {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicToListAsyncShould));
            _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig,
                new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model));
            _repository.CreateTableAsync();
        }

    [SetUp]
    public async Task SetUp()
    {
            await _repository!.TruncateAsync();
            await _repository!.AddRangeAsync(new[]
            {
                new Car() { Brand = "Volvo", Model = "XC40" },
                new Car() { Brand = "Volvo", Model = "XC60" }
            });
        }
        
    [Test]
    public async Task Returns_All_Elements()
    {
            var res = await _repository.ToListAsync();

            Assert.That(res.Count, Is.EqualTo(2));
        }
}