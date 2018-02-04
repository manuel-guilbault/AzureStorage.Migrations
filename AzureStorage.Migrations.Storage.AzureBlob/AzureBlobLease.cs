using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;

namespace AzureStorage.Migrations.Storage.AzureBlob
{
    public class AzureBlobLease : IDisposable
    {
        private readonly CloudBlob blob;

        public AzureBlobLease(CloudBlob blob, string leaseId)
        {
            this.blob = blob ?? throw new ArgumentNullException(nameof(blob));
            this.LeaseId = leaseId ?? throw new ArgumentNullException(nameof(leaseId));
        }

        public string LeaseId { get; }
        public bool IsReleased { get; private set; } = false;

        public void Dispose()
        {
            if (!IsReleased)
            {
                blob.ReleaseLease(AccessCondition.GenerateLeaseCondition(LeaseId));
                IsReleased = true;
            }
        }
    }
}
