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
        private string leaseId;

        public AzureBlobStorage(CloudBlockBlob blob)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
        }

        public async Task CreateIfNotExistsAsync()
        {
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
                }
            }
        }

        public async Task<IDisposable> LockAsync()
        {
            leaseId = await blob.AcquireLeaseAsync(null);
            return new AzureBlobLease(blob, leaseId);
        }

        public async Task<ExecutedMigrationCollection> ReadAsync()
        {
            var json = await blob.DownloadTextAsync(
                encoding,
                AccessCondition.GenerateLeaseCondition(leaseId),
                new BlobRequestOptions(),
                new OperationContext());
            var result = JsonConvert.DeserializeObject<ExecutedMigrationCollection>(json, jsonSerializerSettings);
            return result;
        }

        public async Task WriteAsync(ExecutedMigrationCollection executedMigrations)
        {
            await WriteAsync(
                executedMigrations,
                AccessCondition.GenerateLeaseCondition(leaseId));
        }

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
