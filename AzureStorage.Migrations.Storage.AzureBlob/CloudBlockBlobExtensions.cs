using AzureStorage.Migrations.Runner.Storage;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Storage.AzureBlob
{
    internal static class CloudBlockBlobExtensions
    {
        private const string contentType = "application/json";
        private static readonly Encoding encoding = Encoding.UTF8;
        private static readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings();

        public static async Task<ExecutedMigrationCollection> ReadAsync(this CloudBlockBlob blob, AccessCondition accessCondition, CancellationToken cancellationToken = default)
        {
            var json = await blob.DownloadTextAsync(
                encoding,
                accessCondition,
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);
            var result = JsonConvert.DeserializeObject<ExecutedMigrationCollection>(json, jsonSerializerSettings);
            return result;
        }

        public static async Task WriteAsync(this CloudBlockBlob blob, ExecutedMigrationCollection executedMigrations, AccessCondition accessCondition, CancellationToken cancellationToken)
        {
            blob.Properties.ContentType = contentType;
            var json = JsonConvert.SerializeObject(executedMigrations, jsonSerializerSettings);
            await blob.UploadTextAsync(
                json,
                encoding,
                accessCondition,
                new BlobRequestOptions(),
                new OperationContext(),
                cancellationToken);
        }
    }
}
