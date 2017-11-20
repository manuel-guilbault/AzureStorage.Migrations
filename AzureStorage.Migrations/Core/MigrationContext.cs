using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AzureStorage.Migrations.Core
{
    public class MigrationContext
    {
        public MigrationContext(CloudStorageAccount account)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            BlobClient = account.CreateCloudBlobClient();
            TableClient = account.CreateCloudTableClient();
        }

        public CloudStorageAccount Account { get; }
        public CloudBlobClient BlobClient { get; }
        public CloudTableClient TableClient { get; }
    }
}
