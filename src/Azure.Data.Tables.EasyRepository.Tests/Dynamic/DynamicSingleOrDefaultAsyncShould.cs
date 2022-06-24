using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic
{
    public class DynamicSingleOrDefaultAsyncShould
    {
        private DynamicTableRepository<Car>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicSingleOrDefaultAsyncShould));
            _repository = new DynamicTableRepository<Car>(serviceClient, tableConfig, 
                new TableEntityAdapter<Car>(x => x.Brand[..1], x => x.Model));
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task TearDown()
        {
            await _repository!.TruncateAsync();
            await _repository!.AddRangeAsync(new[]
            {
                new Car() { Brand = "Volvo", Model = "XC40" }
            });
        }

        [Test]
        public async Task Return_Null_When_Filter_Does_Not_Match()
        {
            var res = await _repository!.SingleOrDefaultAsync("P", "911");
            Assert.That(res, Is.Null);
        }

        [Test]
        public async Task Return_The_Element()
        {
            var res = await _repository!.SingleOrDefaultAsync("V", "XC40");
            Assert.Multiple(() =>
            {
                Assert.That(res, Is.Not.Null);
                Assert.That(res!.Brand, Is.EqualTo("Volvo"));
                Assert.That(res.Model, Is.EqualTo("XC40"));
            });
        }
    }
}