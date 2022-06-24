using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class SingleAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(SingleAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            await _repository!.TruncateAsync();
            await _repository.AddRangeAsync(new[]
            {
                new Product {PartitionKey = "treats", RowKey = "cola", Weight = 128}
            });
        }

        [Test]
        public void Throws_When_Filter_Does_Not_Match()
        {
            Assert.That(async () => await _repository!.SingleAsync("treats", "shirt"), Throws.InstanceOf<EntityNotFoundException<Product>>());
        }

        [Test]
        public async Task Return_The_Element()
        {
            var res = await _repository!.SingleAsync("treats", "cola");
            Assert.Multiple(() =>
            {
                Assert.That(res, Is.Not.Null);
                Assert.That(res.PartitionKey, Is.EqualTo("treats"));
                Assert.That(res.RowKey, Is.EqualTo("cola"));
                Assert.That(res.Weight, Is.EqualTo(128));
            });
        }
    }
}