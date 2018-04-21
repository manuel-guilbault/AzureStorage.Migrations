using Microsoft.WindowsAzure.Storage.Table;
using System;
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
    }
}
