using System;
using System.Linq;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class AddAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(AddAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
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
            await _repository!.AddAsync(new Product { PartitionKey = "treats", RowKey = "tagada" });

            var retrievedProducts = await _repository.ToListAsync();

            Assert.Multiple(() =>
            {
                Assert.That(retrievedProducts.Count, Is.EqualTo(1));
                Assert.That(retrievedProducts.First().PartitionKey == "treats");
                Assert.That(retrievedProducts.First().RowKey == "tagada");
            });
        }
    }
}