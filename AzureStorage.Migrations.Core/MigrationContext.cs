using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace AzureStorage.Migrations.Core
{
    public class MigrationContext
    {
        public MigrationContext(CloudStorageAccount account, string[] tags, IDictionary<string, string> properties)
        {
            Account = account ?? throw new ArgumentNullException(nameof(account));
            BlobClient = account.CreateCloudBlobClient();
            TableClient = account.CreateCloudTableClient();
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public CloudStorageAccount Account { get; }
        public CloudBlobClient BlobClient { get; }
        public CloudTableClient TableClient { get; }
        public string[] Tags { get; }
        public IDictionary<string, string> Properties { get; }
    }
}
