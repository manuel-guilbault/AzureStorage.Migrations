using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace AzureStorage.Migrations.Storage
{
    public class AzureBlobLease : IDisposable
    {
        private readonly CloudBlob blob;
        private readonly string leaseId;
        private bool isReleased = false;

        public AzureBlobLease(CloudBlob blob, string leaseId)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
            this.leaseId = leaseId ?? throw new ArgumentNullException(nameof(leaseId));
        }

        public void Dispose()
        {
            if (!isReleased)
            {
                blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(leaseId));
                isReleased = true;
            }
        }
    }
}
