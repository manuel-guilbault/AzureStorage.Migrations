using AzureStorage.Migrations.Core;
using AzureStorage.Migrations.Runner;
using AzureStorage.Migrations.Storage.AzureBlob;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Console
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var hostBuilder = new HostBuilder()
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config.AddEnvironmentVariables();
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var settings = SettingsParser.Parse(args);

                    services
                        .AddScoped(_ => CloudStorageAccount.Parse(settings.ConnectionString))
                        .AddScoped(p => new MigrationContext(
                            p.GetRequiredService<CloudStorageAccount>(),
                            settings.Tags,
                            settings.Properties))
                        .AddScoped(p =>
                        {
                            var context = p.GetRequiredService<MigrationContext>();
                            var container = context.BlobClient.GetContainerReference(settings.Container);
                            var blob = container.GetBlockBlobReference(settings.Blob);
                            var storage = new AzureBlobStorage(blob);

                            var assembly = LoadAssembly(settings.Assembly);

                            return new MigrationRunner(
                                storage,
                                new DefaultMigrationFinder(
                                    new DefaultMigrationFactory(),
                                    assembly));
                        })
                    ;
                })
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                    logging.AddConsole();
                })
                .UseConsoleLifetime()
            ;

            using (var host = hostBuilder.Build())
            {
                var applicationLifetime = host.Services.GetRequiredService<IApplicationLifetime>();
                await host.StartAsync();

                using (var scope = host.Services.CreateScope())
                {
                    var runner = host.Services.GetRequiredService<MigrationRunner>();
                    var context = host.Services.GetRequiredService<MigrationContext>();

                    await runner.RunAsync(context, applicationLifetime.ApplicationStopping);
                }

                await host.StopAsync();
            }
        }
        
        private static Assembly LoadAssembly(string assemblyPath)
        {
            var assemblyAbsolutePath = Path.GetFullPath(assemblyPath);
            return AssemblyLoader.Load(new FileInfo(assemblyAbsolutePath));
        }
    }
}
