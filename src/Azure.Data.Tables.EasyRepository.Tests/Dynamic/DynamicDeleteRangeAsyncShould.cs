using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic
{
    public class DynamicDeleteRangeAsyncShould
    {
        private DynamicTableRepository<Car>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DynamicDeleteRangeAsyncShould));
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
                new Car { Brand = "Volvo", Model = "XC40" },
                new Car { Brand = "Volvo", Model = "XC60" },
                new Car { Brand = "Fiat", Model = "Punto" },
            });
        }

        [Test]
        public void Does_Not_Throws_When_Removing_Zero_Elements()
        {
            Assert.That(async () => await _repository!.DeleteRangeAsync(new List<Car>()), Throws.Nothing);
        }

        [Test]
        public void Remove_One_Element()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(async () => await _repository!.DeleteRangeAsync(new[]
                {
                    new Car{Brand = "Volvo", Model = "XC40"}
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "V");
                Assert.That(result.Single().Model, Is.EqualTo("XC60"));
            });
        }

        [Test]
        public void Remove_Multiple_Elements_On_Same_Partition()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(async () => await _repository!.DeleteRangeAsync(new[]
                {
                    new Car{Brand = "Volvo", Model = "XC40"},
                    new Car{Brand = "Volvo", Model = "XC60"}
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "V");
                Assert.That(result.Count, Is.EqualTo(0));
            });
        }

        [Test]
        public void Remove_Multiple_Elements_On_Different_Partition()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(async () => await _repository!.DeleteRangeAsync(new[]
                {
                    new Car{Brand = "Volvo", Model = "XC40"},
                    new Car{Brand = "Fiat", Model = "Punto"},
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "V" || x.PartitionKey == "F");
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.Single().Model, Is.EqualTo("XC60"));
            });
        }
    }
}