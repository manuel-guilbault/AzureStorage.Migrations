using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Core
{
    public static class CloudBlobExtensions
    {
        public static async Task ForEachAsync<T>(this CloudBlobContainer container, Func<T, Task> action)
            where T : class, IListBlobItem
            => await container.ForEachAsync(async item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    await action(typedItem);
                }
            });

        public static async Task ForEachAsync<T>(this CloudBlobContainer container, Action<T> action)
            where T : class, IListBlobItem
            => await container.ForEachAsync(item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    action(typedItem);
                }
            });

        public static async Task ForEachAsync(this CloudBlobContainer container, Action<IListBlobItem> action)
            => await container.ForEachAsync(item =>
            {
                action(item);
                return Task.CompletedTask;
            });

        public static async Task ForEachAsync(this CloudBlobContainer container, Func<IListBlobItem, Task> action)
        {
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await container.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                {
                    await action(item);
                }
            }
            while (continuationToken != null);
        }

        public static async Task ForEachAsync<T>(this CloudBlobDirectory directory, Action<T> action)
            where T : class, IListBlobItem
            => await directory.ForEachAsync(item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    action(typedItem);
                }
            });

        public static async Task ForEachAsync<T>(this CloudBlobDirectory directory, Func<T, Task> action)
            where T : class, IListBlobItem
            => await directory.ForEachAsync(async item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    await action(typedItem);
                }
            });

        public static async Task ForEachAsync(this CloudBlobDirectory directory, Action<IListBlobItem> action)
            => await directory.ForEachAsync(item =>
            {
                action(item);
                return Task.CompletedTask;
            });

        public static async Task ForEachAsync(this CloudBlobDirectory directory, Func<IListBlobItem, Task> action)
        {
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await directory.ListBlobsSegmentedAsync(continuationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                {
                    await action(item);
                }
            }
            while (continuationToken != null);
        }
    }
}
