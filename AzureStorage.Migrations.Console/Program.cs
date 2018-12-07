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
            var context = new MigrationContext(storageAccount, settings.Tags, settings.Properties);

            var runner = await CreateRunnerAsync(settings, context);
            await runner.RunAsync(context);
        }

        private static Settings ParseSettings(string[] args)
        {
            var parser = new SettingsParser();
            var settings = parser.Parse(args);
            return settings;
        }

        private static async Task<MigrationRunner> CreateRunnerAsync(Settings settings, MigrationContext context)
        {
            var blob = GetBlob(context.BlobClient, settings);
            var storage = new AzureBlobStorage(blob);
            await storage.CreateIfNotExistsAsync();

            var assembly = LoadAssembly(settings.Assembly);

            var runner = new MigrationRunner(
                storage,
                new DefaultMigrationFinder(
                    new DefaultMigrationFactory(),
                    assembly));
            return runner;
        }

        private static CloudBlockBlob GetBlob(CloudBlobClient blobClient, Settings settings)
        {
            var container = blobClient.GetContainerReference(settings.Container);
            var blob = container.GetBlockBlobReference(settings.Blob);
            return blob;
        }

        private static Assembly LoadAssembly(string assemblyPath)
        {
            var assemblyAbsolutePath = Path.GetFullPath(assemblyPath);
            return AssemblyLoader.Load(new FileInfo(assemblyAbsolutePath));
        }
    }
}
