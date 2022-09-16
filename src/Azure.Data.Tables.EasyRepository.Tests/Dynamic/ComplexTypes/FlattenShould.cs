using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.ComplexTypes
{
    public class FlattenShould
    {
        private DynamicTableRepository<Client>? _repository;

        [OneTimeSetUp]
        public void OneTime()
        {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(FlattenShould));
            var adapter = new TableEntityAdapter<Client>(x => x.Name[..1], x => x.Name);
            adapter.Flatten(x => x.Address);
            _repository = new DynamicTableRepository<Client>(serviceClient, tableConfig, adapter);
            _repository.CreateTableAsync();
        }

        [SetUp]
        public async Task TearDown()
        {
            await _repository!.TruncateAsync();
        }

        [Test]
        public async Task Support_Round_Trip()
        {
            var client = new Client
            {
                Name = "Coca Cola",
                Address = new Address
                {
                    Row1 = "Societe Coca Cola",
                    Row2 = "9 Chem.de Bretagne",
                    City = "Issy-les-Moulineaux",
                    Country = "France"
                }
            };
            await _repository!.AddAsync(client);

            var retrievedClient = await _repository.SingleAsync("C", "Coca Cola");

            Assert.Multiple(() =>
            {
                Assert.That(retrievedClient.Name, Is.EqualTo(client.Name));
                Assert.That(retrievedClient.Address.Row1, Is.EqualTo(client.Address.Row1));
                Assert.That(retrievedClient.Address.Row2, Is.EqualTo(client.Address.Row2));
                Assert.That(retrievedClient.Address.City, Is.EqualTo(client.Address.City));
                Assert.That(retrievedClient.Address.Country, Is.EqualTo(client.Address.Country));
            }); 
        }
    }
}