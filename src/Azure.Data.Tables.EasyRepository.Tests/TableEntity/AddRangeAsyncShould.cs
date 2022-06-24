using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity
{
    public class AddRangeAsyncShould
    {
        private TableEntityRepository<Product>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(AddRangeAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task TearDown()
        {
            await _repository!.TruncateAsync();
        }

        [Test]
        public void Not_Throws_When_Adding_Zero_Elements()
        {
            Assert.That(async () => await _repository!.AddRangeAsync(new List<Product>()), Throws.Nothing);
        }

        [Test]
        public void Add_One_Element()
        {
            Assert.That(async () => await _repository!.AddRangeAsync(new []
            {
                new Product("treats", "cola")
            }), Throws.Nothing);
        }

        [Test]
        public void Add_Multiple_Elements_On_Same_Partition()
        {
            Assert.That(async () => await _repository!.AddRangeAsync(new[]
            {
                new Product("treats", "cola"),
                new Product("treats", "tagada"),
            }), Throws.Nothing);
        }

        [Test]
        public void Add_Multiple_Elements_On_Different_Partitions()
        {
            Assert.That(async () => await _repository!.AddRangeAsync(new[]
            {
                new Product("treats", "cola"),
                new Product("treats", "tagada"),
                new Product("trousers", "skiny"),
            }), Throws.Nothing);
        }
    }
}