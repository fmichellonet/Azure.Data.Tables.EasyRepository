using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class DeleteRangeAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(DeleteRangeAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task TearDown()
        {
            await _repository!.TruncateAsync();
            await _repository!.AddRangeAsync(new[]
            {
                new Product("treats", "cola"),
                new Product("treats", "tagada"),
                new Product("trousers", "skiny")
            });
        }

        [Test]
        public void Does_Not_Throws_When_Removing_Zero_Elements()
        {
            Assert.That(async () => await _repository!.DeleteRangeAsync(new List<Product>()), Throws.Nothing);
        }

        [Test]
        public void Remove_One_Element()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(async () => await _repository!.DeleteRangeAsync(new[]
                {
                    new Product { PartitionKey = "treats", RowKey = "cola"}
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "treats");
                Assert.That(result.Single().RowKey, Is.EqualTo("tagada"));
            });
        }

        [Test]
        public void Remove_Multiple_Elements_On_Same_Partition()
        {
            Assert.Multiple(async () =>
            {
                Assert.That(async () => await _repository!.DeleteRangeAsync(new[]
                {
                    new Product { PartitionKey = "treats", RowKey = "cola"},
                    new Product { PartitionKey = "treats", RowKey = "tagada"}
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "treats");
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
                    new Product { PartitionKey = "treats", RowKey = "cola"},
                    new Product { PartitionKey = "trousers", RowKey = "skiny"}
                }), Throws.Nothing);

                var result = await _repository!.WhereAsync(x => x.PartitionKey == "treats" || x.PartitionKey == "trousers");
                Assert.That(result.Count, Is.EqualTo(1));
                Assert.That(result.Single().RowKey, Is.EqualTo("tagada"));
            });
        }
    }
}