using AzureStorage.Migrations.Runner.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Storage.AzureBlob
{
    public class AzureBlobStorage : IExecutedMigrationsStorage
    {
        private const string contentType = "application/json";
        private static readonly Encoding encoding = Encoding.UTF8;
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

        private readonly CloudBlockBlob blob;
        private AzureBlobLease lease;

        public AzureBlobStorage(CloudBlockBlob blob)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
        }

        private bool IsLocked => lease != null && !lease.IsReleased;

        public async Task CreateIfNotExistsAsync()
        {
            await blob.Container.CreateIfNotExistsAsync(
                BlobContainerPublicAccessType.Off,
                new BlobRequestOptions(),
                new OperationContext());

            var exists = await blob.ExistsAsync();
            if (!exists)
            {
                blob.Properties.ContentType = contentType;
                try
                {
                    await WriteAsync(
                        new ExecutedMigrationCollection(),
                        AccessCondition.GenerateIfNotExistsCondition());
                }
                catch (StorageException e)
                    when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    // Blob was created by someone else after the call to ExistsAsync(), 
                    // so just ignore the exception.
                }
            }
        }

        public async Task<IDisposable> LockAsync()
        {
            if (IsLocked)
            {
                throw new InvalidOperationException($"Storage is already locked.");
            }

            var leaseId = await blob.AcquireLeaseAsync(null);
            lease = new AzureBlobLease(blob, leaseId);
            return lease;
        }

        public async Task<ExecutedMigrationCollection> ReadAsync()
        {
            var json = await blob.DownloadTextAsync(
                encoding,
                CreateLeasedAccessCondition(),
                new BlobRequestOptions(),
                new OperationContext());
            var result = JsonConvert.DeserializeObject<ExecutedMigrationCollection>(json, jsonSerializerSettings);
            return result;
        }

        public async Task WriteAsync(ExecutedMigrationCollection executedMigrations)
        {
            await WriteAsync(
                executedMigrations,
                CreateLeasedAccessCondition());
        }

        private AccessCondition CreateLeasedAccessCondition()
            => IsLocked
                ? AccessCondition.GenerateLeaseCondition(lease.LeaseId)
                : AccessCondition.GenerateEmptyCondition();

        private async Task WriteAsync(ExecutedMigrationCollection executedMigrations, AccessCondition accessCondition)
        {
            var json = JsonConvert.SerializeObject(executedMigrations, jsonSerializerSettings);
            await blob.UploadTextAsync(
                json,
                encoding,
                accessCondition,
                new BlobRequestOptions(),
                new OperationContext());
        }
    }
}
