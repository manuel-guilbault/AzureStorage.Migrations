using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Reflection;
using System.Threading.Tasks;
using AzureStorage.Migrations.Core;
using AzureStorage.Migrations.Storage.AzureBlob;
using AzureStorage.Migrations.Runner;
using System.IO;

namespace AzureStorage.Migrations.Console
{
    static class Program
    {
        static void Main(params string[] args)
        {
            MainAsync(args).GetAwaiter().GetResult();
        }

        static async Task MainAsync(params string[] args)
        {
            var settings = ParseSettings(args);

            var storageAccount = CloudStorageAccount.Parse(settings.ConnectionString);
            var context = new MigrationContext(storageAccount);

            var blob = await CreateBlobAsync(context.BlobClient, settings);
            var storage = new AzureBlobStorage(blob);
            await storage.CreateIfNotExistsAsync();

            var assembly = Assembly.LoadFile(Path.GetFullPath(settings.Assembly));
            var runner = new MigrationRunner(
                storage,
                new DefaultMigrationFinder(
                    new DefaultMigrationFactory(),
                    assembly));
            await runner.RunAsync(context, settings.Tags);
        }

        private static Settings ParseSettings(string[] args)
        {
            var parser = new SettingsParser();
            var settings = parser.Parse(args);
            return settings;
        }

        private static async Task<CloudBlockBlob> CreateBlobAsync(CloudBlobClient blobClient, Settings settings)
        {
            var storageContainer = blobClient.GetContainerReference(settings.Container);
            await storageContainer.CreateIfNotExistsAsync();

            var blob = storageContainer.GetBlockBlobReference(settings.Blob);
            return blob;
        }
    }
}
