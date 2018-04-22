using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Core
{
    public static class CloudTableExtensions
    {
        public static async Task ForEachAsync(this CloudTable table, TableQuery query, Action<DynamicTableEntity> action)
            => await table.ForEachAsync(query, row =>
            {
                action(row);
                return Task.CompletedTask;
            });

        public static async Task ForEachAsync(this CloudTable table, TableQuery query, Func<DynamicTableEntity, Task> action)
        {
            TableContinuationToken token = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, token);
                token = result.ContinuationToken;
                foreach (var row in result)
                {
                    await action(row);
                }
            }
            while (token != null);
        }

        public static async Task ForEachAsync<T>(this CloudTable table, TableQuery<T> query, Action<T> action)
            where T : class, ITableEntity, new()
            => await table.ForEachAsync(query, row =>
            {
                action(row);
                return Task.CompletedTask;
            });

        public static async Task ForEachAsync<T>(this CloudTable table, TableQuery<T> query, Func<T, Task> action)
            where T: class, ITableEntity, new()
        {
            TableContinuationToken token = null;
            do
            {
                var result = await table.ExecuteQuerySegmentedAsync(query, token);
                token = result.ContinuationToken;
                foreach (var row in result)
                {
                    await action(row);
                }
            }
            while (token != null);
        }

        public static async Task UpdateAsync(this CloudTable table, TableQuery query, Action<DynamicTableEntity> mutator)
            => await table.UpdateAsync(query, row =>
            {
                mutator(row);
                return Task.CompletedTask;
            });

        public static async Task UpdateAsync(this CloudTable table, TableQuery query, Func<DynamicTableEntity, Task> mutator)
            => await table.ForEachAsync(query, async row =>
            {
                var updated = Clone(row);
                await mutator(updated);
                if (!AreEqual(row, updated))
                {
                    await table.ExecuteAsync(TableOperation.Replace(updated));
                }
            });

        private static DynamicTableEntity Clone(DynamicTableEntity row)
            => new DynamicTableEntity(
                row.PartitionKey,
                row.RowKey,
                row.ETag,
                row.Properties.ToDictionary(x => x.Key, x => x.Value));

        private static bool AreEqual(DynamicTableEntity row1, DynamicTableEntity row2)
            => row1.PartitionKey == row2.PartitionKey
                && row1.RowKey == row2.RowKey
                && row1.ETag == row2.ETag
                && row1.Properties.Keys.SequenceEqual(row2.Properties.Keys)
                && row1.Properties.All(p => Equals(p.Value, row2.Properties[p.Key]));
    }
}
