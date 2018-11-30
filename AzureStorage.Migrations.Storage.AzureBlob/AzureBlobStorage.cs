using AzureStorage.Migrations.Runner.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Storage.AzureBlob
{
    public class AzureBlobStorage : IExecutedMigrationsStorage
    {
        private readonly CloudBlockBlob blob;

        public AzureBlobStorage(CloudBlockBlob blob)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
        }
        
        private async Task CreateIfNotExistsAsync(CancellationToken cancellationToken)
        {
            await blob.Container.CreateIfNotExistsAsync(
                BlobContainerPublicAccessType.Off,
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);

            var exists = await blob.ExistsAsync(
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);
            if (!exists)
            {
                try
                {
                    await blob.WriteAsync(
                        new ExecutedMigrationCollection(),
                        AccessCondition.GenerateIfNotExistsCondition(),
                        cancellationToken);
                }
                catch (StorageException e)
                    when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.PreconditionFailed)
                {
                    // Blob was created by someone else after the call to ExistsAsync(), 
                    // so just ignore the exception.
                }
            }
        }

        public async Task RunWithLeaseAsync(Func<ILeasedExecutedMigrationsStorage, Task> action, CancellationToken cancellationToken = default)
        {
            await CreateIfNotExistsAsync(cancellationToken);

            string leaseId = null;
            try
            {
                leaseId = await blob.AcquireLeaseAsync(
                    null,
                    null,
                    AccessCondition.GenerateEmptyCondition(),
                    new BlobRequestOptions(),
                    new OperationContext(),
                    cancellationToken);
            }
            catch (StorageException e)
                when (e.RequestInformation.HttpStatusCode == (int)HttpStatusCode.Conflict)
            {
                throw new InvalidOperationException($"The lease for blob {blob.Uri.AbsolutePath} is already leased.");
            }

            try
            {
                var leasedStorage = new AzureBlobLeasedExecutedMigrationsStorage(blob, leaseId);
                await action(leasedStorage);
            }
            finally
            {
                await blob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition(leaseId));
            }
        }
    }
}
