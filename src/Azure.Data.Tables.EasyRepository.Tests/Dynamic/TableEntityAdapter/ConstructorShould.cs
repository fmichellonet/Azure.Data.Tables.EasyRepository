using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Azure.Data.Tables.EasyRepository.Tests.Dynamic.Models;
using NUnit.Framework;

namespace Azure.Data.Tables.EasyRepository.Tests.Dynamic.TableEntityAdapter
{
    public class ConstructorShould
    {
        private TableServiceClient? _serviceClient;
        private TableConfiguration? _tableConfig;

        [OneTimeSetUp]
        public void OneTime()
        {
            _serviceClient = new TableServiceClient("UseDevelopmentStorage=true");
            _tableConfig = new TableConfiguration(nameof(ConstructorShould));
            var repository = new DynamicTableRepository<Product>(_serviceClient, _tableConfig, BuildAdapter());
            repository.CreateTableAsync();
        }
        private static TableEntityAdapter<Product> BuildAdapter()
        {
            var adapter = new TableEntityAdapter<Product>(x => x.Name[..1], x => x.Name);
            adapter.UseSerializerFor(x => x.Weight);
            return adapter;
        }

        [SetUp]
        public async Task TearDown()
        {
            var repository = new DynamicTableRepository<Product>(_serviceClient, _tableConfig, BuildAdapter());
            await repository!.TruncateAsync();
        }

        /// <summary>
        /// Ensure <code> Expression<Func<TEntity, TProperty>> </code> is compiled just once,
        /// and globally accessible per Serializer / Entity / Property
        /// </summary>
        /// <param name="count"></param>
        [Ignore("TableServiceClient.GetTableClient now takes forever")]
        [TestCase(100000)]
        public void TakeLittleTime(int count)
        {
            var sw = Stopwatch.StartNew();
            for (var i = 0; i < count; i++)
            {
                var repository = new DynamicTableRepository<Product>(_serviceClient, _tableConfig, BuildAdapter());
            }
            sw.Stop();

            Assert.That(sw.Elapsed.Seconds, Is.LessThan(1));

            Console.WriteLine($"{sw.ElapsedMilliseconds} ms => {sw.Elapsed.Seconds} s");
        }
    }
}