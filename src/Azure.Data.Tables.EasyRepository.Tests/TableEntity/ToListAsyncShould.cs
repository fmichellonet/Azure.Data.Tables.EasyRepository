using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.TableEntity.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.TableEntity;

[TestFixture]
public class ToListAsyncShould
{

    private TableEntityRepository<Product>? _repository;

    [OneTimeSetUp]
    public void OneTime()
    {
            var serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            var tableConfig = new TableConfiguration(nameof(ToListAsyncShould));
            _repository = new TableEntityRepository<Product>(serviceClient, tableConfig);
            _repository.CreateTableAsync();
        }

    [SetUp]
    public async Task SetUp()
    {
            await _repository!.TruncateAsync();
            await _repository!.AddRangeAsync(new[]
            {
                new Product { PartitionKey = "treats", RowKey = "cola", Weight = 125},
                new Product { PartitionKey = "treats", RowKey = "tagada", Weight = 150}
            });
        }
        
    [Test]
    public async Task Returns_All_Elements()
    {
            var res = await _repository!.ToListAsync();

            Assert.That(res.Count, Is.EqualTo(2));
        }
}