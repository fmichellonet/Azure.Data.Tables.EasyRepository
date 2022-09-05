using System.Text.Json;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Serialization;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.ComplexTypes
{
    public class CustomSerializerShould
    {
        private class UnitSwapperSerializer : ISerializer
        {
            public string Serialize<TProperty>(TProperty item)
            {
                var str = JsonSerializer.Serialize(item);
                if (item is Weight)
                {
                    str = str.Replace("\"Unit\":1", "\"Unit\":0");
                }

                return str;
            }
            
            public TProperty Deserialize<TProperty>(string value)
            {
                return JsonSerializer.Deserialize<TProperty>(value);
            }
        }

        private DynamicTableRepository<Product>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(CustomSerializerShould));
            var adapter = new TableEntityAdapter<Product>(x => x.Name[..1], x => x.Name);
            adapter.UseSerializerFor<UnitSwapperSerializer, Weight>(x => x.Weight);
            _repository = new DynamicTableRepository<Product>(serviceClient, tableConfig, adapter);
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task TearDown()
        {
            await _repository!.TruncateAsync();
        }

        [Test]
        public async Task Support_Custom_Serialization_Deserialization_Round_Trip()
        {
            var product = new Product("Iron", new Weight(1500, Unit.g));
            await _repository!.AddAsync(product);

            var retrievedProduct = await _repository.SingleAsync("I", "Iron");

            Assert.Multiple(() =>
            {
                Assert.That(retrievedProduct.Name, Is.EqualTo(product.Name));
                Assert.That(retrievedProduct.Weight.Value, Is.EqualTo(product.Weight.Value));
                Assert.That(retrievedProduct.Weight.Unit, Is.EqualTo(Unit.Kg));
            }); 
        }
    }
}