using AzureStorage.Migrations.Core.Tests.Bootstrapping;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using NFluent;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace AzureStorage.Migrations.Core.Tests
{
    public class CloudTableExtensionsTests : IDisposable
    {
        private readonly CloudStorageAccount account;
        private readonly CloudTableClient tableClient;
        private readonly CloudTable table;

        public CloudTableExtensionsTests()
        {
            account = CloudStorageAccount.Parse(Settings.GetStorageConnectionString());
            tableClient = account.CreateCloudTableClient();
            table = tableClient.GetTableReference($"test{Guid.NewGuid().ToString().Replace("-", "")}");
            table.CreateIfNotExists();
        }
        
        public void Dispose()
        {
            table.DeleteIfExists();
        }

        private DynamicTableEntity CreateRow(string id = null)
        {
            id = id ?? Guid.NewGuid().ToString();
            return new DynamicTableEntity(id, id)
            {
                Properties = new Dictionary<string, EntityProperty>()
                {
                    ["value"] = EntityProperty.GeneratePropertyForString(id),
                },
            };
        }

        [Fact]
        public async Task Should_Update_Only_Modified_Rows()
        {
            var idNotToUpdate = Guid.NewGuid().ToString();
            var insertResultNotToUpdate = await table.ExecuteAsync(TableOperation.Insert(CreateRow(idNotToUpdate)));

            var idToUpdate = Guid.NewGuid().ToString();
            var insertResultToUpdate = await table.ExecuteAsync(TableOperation.Insert(CreateRow(idToUpdate)));

            var newId = Guid.NewGuid().ToString();
            await table.UpdateAsync(new TableQuery(), row =>
            {
                if (row.Properties["value"].StringValue == idToUpdate)
                {
                    row.Properties["value"] = EntityProperty.GeneratePropertyForString(newId);
                }
            });

            var notUpdatedRow = await table.ExecuteAsync(TableOperation.Retrieve(idNotToUpdate, idNotToUpdate));
            Check.That(notUpdatedRow.Etag).IsEqualTo(insertResultNotToUpdate.Etag);

            var updatedRow = await table.ExecuteAsync(TableOperation.Retrieve(idToUpdate, idToUpdate));
            Check.That(updatedRow.Etag).Not.IsEqualTo(insertResultToUpdate.Etag);
            Check.That(((DynamicTableEntity)updatedRow.Result).Properties["value"]).IsEqualTo(EntityProperty.GeneratePropertyForString(newId));
        }

        [Fact]
        public async Task ForEach_Should_Support_Multiple_ResultSet_Pages()
        {
            const int batches = 5;
            const int batchSize = 100;
            const int iterations = batches * batchSize;

            for (var i = 0; i < batches; ++i)
            {
                var partitionKey = Guid.NewGuid().ToString();
                var batch = new TableBatchOperation();
                for (var j = 0; j < batchSize; ++j)
                {
                    batch.Insert(new DynamicTableEntity(partitionKey, Guid.NewGuid().ToString()));
                }
                await table.ExecuteBatchAsync(batch);
            }

            var count = 0;
            await table.ForEachAsync(new TableQuery(), _ => ++count);

            Check.That(count).IsEqualTo(iterations);
        }
    }
}
