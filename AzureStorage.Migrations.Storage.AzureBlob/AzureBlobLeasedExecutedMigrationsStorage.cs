using AzureStorage.Migrations.Runner.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Storage.AzureBlob
{
    public class AzureBlobLeasedExecutedMigrationsStorage : ILeasedExecutedMigrationsStorage
    {
        private readonly CloudBlockBlob blob;
        private readonly string leaseId;

        public AzureBlobLeasedExecutedMigrationsStorage(CloudBlockBlob blob, string leaseId)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
            this.leaseId = leaseId ?? throw new ArgumentNullException(nameof(leaseId));
        }

        public async Task<ExecutedMigrationCollection> ReadAsync(CancellationToken cancellationToken = default) 
            => await blob.ReadAsync(AccessCondition.GenerateLeaseCondition(leaseId), cancellationToken);

        public async Task WriteAsync(
            ExecutedMigrationCollection executedMigrations, 
            CancellationToken cancellationToken = default) 
            => await blob.WriteAsync(
                executedMigrations,
                AccessCondition.GenerateLeaseCondition(leaseId),
                cancellationToken);
    }
}
