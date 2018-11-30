using AzureStorage.Migrations.Core;
using AzureStorage.Migrations.Runner.Storage;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner
{
    public class MigrationRunner
    {
        private readonly IExecutedMigrationsStorage storage;
        private readonly IMigrationFinder finder;

        public MigrationRunner(IExecutedMigrationsStorage storage, IMigrationFinder finder)
        {
            this.storage = storage ?? throw new ArgumentNullException(nameof(storage));
            this.finder = finder ?? throw new ArgumentNullException(nameof(finder));
        }

        public async Task RunAsync(MigrationContext context, CancellationToken cancellationToken = default)
        {
            var migrationDefinitions = await finder.FindMigrationsAsync(cancellationToken);

            await storage.RunWithLeaseAsync(async (leasedStorage) =>
            {
                var executedMigrations = await leasedStorage.ReadAsync(cancellationToken);
                var migrationsToExecute = migrationDefinitions.FindMigrationsToExecute(
                    executedMigrations,
                    context.Tags);

                Trace.TraceInformation($"Starting execution of {migrationsToExecute.Count} migrations.");
                foreach (var definition in migrationsToExecute.OrderBy(x => x.Version))
                {
                    Trace.TraceInformation($"Executing migration #{definition.Version} ({definition.Migration.GetType()}).");
                    await definition.Migration.ExecuteAsync(context);

                    executedMigrations = executedMigrations.MergeWith(definition.AsExecuted());
                    await leasedStorage.WriteAsync(executedMigrations);

                    cancellationToken.ThrowIfCancellationRequested();
                }
                Trace.TraceInformation($"Done.");
            }, cancellationToken);
        }
    }
}
