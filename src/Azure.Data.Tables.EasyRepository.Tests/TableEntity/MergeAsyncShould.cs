using System;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class MergeAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public async Task OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(MergeAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            await _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task SetUp()
        {
            await _repository!.TruncateAsync();
            await _repository!.AddAsync(new Product
            {
                PartitionKey = "treats",
                RowKey = "cola",
                Weight = 125
            });
        }

        [Test]
        public void Throws_ArgumentNullException_When_Passing_Null()
        {
            Assert.That(async () => await _repository!.MergeAsync(null), Throws.InstanceOf<ArgumentNullException>());
        }

        [Test]
        public async Task Merge_One_Element()
        {
            await _repository!.MergeAsync(new Product
            {
                PartitionKey = "treats",
                RowKey = "cola",
                Weight = 250,
                ETag = ETag.All
            });

            var retrievedProduct = await _repository.SingleAsync("treats", "cola");

            Assert.Multiple(() =>
            {
                Assert.That(retrievedProduct.Weight == 250);
            });
        }
    }
}