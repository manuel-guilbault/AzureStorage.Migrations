using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Core
{
    public static class CloudBlobExtensions
    {
        public static async Task ForEachAsync<T>(
            this CloudBlobContainer container, 
            Func<T, Task> action, 
            CancellationToken cancellationToken = default)
            where T : class, IListBlobItem
            => await container.ForEachAsync(async item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    await action(typedItem);
                }
            }, cancellationToken);

        public static async Task ForEachAsync<T>(
            this CloudBlobContainer container,
            Action<T> action, 
            CancellationToken cancellationToken = default)
            where T : class, IListBlobItem
            => await container.ForEachAsync(item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    action(typedItem);
                }
            }, cancellationToken);

        public static async Task ForEachAsync(
            this CloudBlobContainer container, 
            Action<IListBlobItem> action, 
            CancellationToken cancellationToken = default)
            => await container.ForEachAsync(item =>
            {
                action(item);
                return Task.CompletedTask;
            }, cancellationToken);

        public static async Task ForEachAsync(
            this CloudBlobContainer container, 
            Func<IListBlobItem, Task> action, 
            CancellationToken cancellationToken = default)
        {
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await container.ListBlobsSegmentedAsync(
                    "",
                    false,
                    new BlobListingDetails(),
                    null,
                    continuationToken,
                    new BlobRequestOptions(),
                    new OperationContext(),
                    cancellationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await action(item);
                }
            }
            while (continuationToken != null);
        }

        public static async Task ForEachAsync<T>(
            this CloudBlobDirectory directory, 
            Action<T> action, 
            CancellationToken cancellationToken = default)
            where T : class, IListBlobItem
            => await directory.ForEachAsync(item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    action(typedItem);
                }
            }, cancellationToken);

        public static async Task ForEachAsync<T>(
            this CloudBlobDirectory directory, 
            Func<T, Task> action, 
            CancellationToken cancellationToken = default)
            where T : class, IListBlobItem
            => await directory.ForEachAsync(async item =>
            {
                var typedItem = item as T;
                if (typedItem != null)
                {
                    await action(typedItem);
                }
            }, cancellationToken);

        public static async Task ForEachAsync(
            this CloudBlobDirectory directory, 
            Action<IListBlobItem> action, 
            CancellationToken cancellationToken = default)
            => await directory.ForEachAsync(item =>
            {
                action(item);
                return Task.CompletedTask;
            }, cancellationToken);

        public static async Task ForEachAsync(
            this CloudBlobDirectory directory, 
            Func<IListBlobItem, Task> action, 
            CancellationToken cancellationToken = default)
        {
            BlobContinuationToken continuationToken = null;
            do
            {
                var response = await directory.ListBlobsSegmentedAsync(
                    false,
                    new BlobListingDetails(),
                    null,
                    continuationToken,
                    new BlobRequestOptions(),
                    new OperationContext(),
                    cancellationToken);
                continuationToken = response.ContinuationToken;
                foreach (var item in response.Results)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    await action(item);
                }
            }
            while (continuationToken != null);
        }

        public static async Task<bool> CreateIfNotExistsAsync(
            this CloudBlobContainer container,
            CancellationToken cancellationToken)
            => await container.CreateIfNotExistsAsync(
                BlobContainerPublicAccessType.Off,
                cancellationToken);

        public static async Task<bool> CreateIfNotExistsAsync(
            this CloudBlobContainer container,
            BlobContainerPublicAccessType publicAccessType,
            CancellationToken cancellationToken)
            => await container.CreateIfNotExistsAsync(
                publicAccessType,
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);

        public static async Task<bool> DeleteIfExistsAsync(
            this CloudBlobContainer container,
            CancellationToken cancellationToken)
            => await container.DeleteIfExistsAsync(
                AccessCondition.GenerateEmptyCondition(),
                cancellationToken);

        public static async Task<bool> DeleteIfExistsAsync(
            this CloudBlobContainer container,
            AccessCondition accessCondition,
            CancellationToken cancellationToken)
            => await container.DeleteIfExistsAsync(
                accessCondition,
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);
    }
}
