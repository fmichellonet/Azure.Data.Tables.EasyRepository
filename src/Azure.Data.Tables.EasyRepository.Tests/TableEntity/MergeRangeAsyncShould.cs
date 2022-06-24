using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class MergeRangeAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public async Task OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(MergeRangeAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig); 
            await _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            await _repository!.TruncateAsync();

            await _repository!.AddRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 125},
                new Product { PartitionKey = "treats", RowKey = "tagada", Weight = 150},
                new Product { PartitionKey = "trousers", RowKey = "skiny", Weight = 1000}
            });
        }

        [Test]
        public void Does_Not_Throws_When_Merging_Zero_Elements()
        {
            Assert.That(async () => await _repository!.MergeRangeAsync(new List<Product>()), Throws.Nothing);
        }


        [Test]
        public async Task Merge_One_Element()
        {
            await _repository!.MergeRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 250}
            });

            var retrievedProduct = await _repository.SingleAsync("treats", "cola");
            Assert.That(retrievedProduct.Weight == 250);
        }

        [Test]
        public async Task Merge_Multiple_Elements_On_Same_Partition()
        {
            Assert.That(async () => await _repository!.MergeRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 300},
                new Product { PartitionKey = "treats", RowKey = "tagada", Weight = 300}
            }), Throws.Nothing);

            var retrievedProducts = await _repository!.WhereAsync(x => x.PartitionKey == "treats");
            
            Assert.That(retrievedProducts.All(x => x.Weight == 300), Is.True);
        }

        [Test]
        public async Task Merge_Multiple_Elements_On_Different_Partitions()
        {
            Assert.That(async () => await _repository!.MergeRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 300},
                new Product { PartitionKey = "trousers", RowKey = "skiny", Weight = 2000}
            }), Throws.Nothing);

            var cola = await _repository!.SingleAsync("treats", "cola");
            var trouser = await _repository!.SingleAsync("trousers", "skiny");

            Assert.That(cola.Weight, Is.EqualTo(300));
            Assert.That(trouser.Weight, Is.EqualTo(2000));
        }
    }
}