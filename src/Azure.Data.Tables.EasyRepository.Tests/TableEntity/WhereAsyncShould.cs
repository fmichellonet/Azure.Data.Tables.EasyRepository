using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    [TestFixture]
    public class WhereAsyncShould
    {

        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(WhereAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            await _repository.CreateTableAsync();
            await _repository!.AddRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 150},
                new Product { PartitionKey = "treats", RowKey = "tagada", Weight = 150},
                new Product { PartitionKey = "trousers", RowKey = "skiny", Weight = 1000}
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
            var res = await _repository!.WhereAsync(x => x.PartitionKey == "treats");

            Assert.That(res.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Be_Queryable_Per_RowKey()
        {
            var res = await _repository!.WhereAsync(x => x.RowKey == "cola");

            Assert.That(res.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Be_Queryable_Per_PartitionKey_And_RowKey()
        {
            var res = await _repository!.WhereAsync(x => x.PartitionKey == "trousers" && x.RowKey == "skiny");

            Assert.That(res.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Be_Queryable_Per_Property()
        {
            var res = await _repository!.WhereAsync(x => x.Weight == 150);

            Assert.That(res.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task Returns_All_Pages()
        {
            var res = await _repository!.WhereAsync(x => x.PartitionKey == "treats", 1);

            Assert.That(res.Count, Is.EqualTo(2));
        }
    }
}
