using AzureStorage.Migrations.Core;
using AzureStorage.Migrations.Runner.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
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

        public async Task RunAsync(MigrationContext context, params string[] tags)
        {
            if (tags == null) { throw new ArgumentNullException(nameof(tags)); }

            using (await storage.LockAsync())
            {
                var executedMigrations = await storage.ReadAsync();

                var migrationsToExecute = FindMigrationsToExecute(executedMigrations, tags);
                await ExecuteAsync(migrationsToExecute, context);
                executedMigrations = AddExecutedMigrations(executedMigrations, migrationsToExecute);

                await storage.WriteAsync(executedMigrations);
            }
        }

        private IEnumerable<MigrationDefinition> FindMigrationsToExecute(ExecutedMigrationCollection executedMigrations, string[] tags)
        {
            var migrations = finder.FindMigrations();

            var migrationsToExecute = migrations
                .Where(m => !executedMigrations.Any(em => em.Version == m.Version))
                .Where(m => m.Matches(tags))
                .ToList();
            return migrationsToExecute;
        }

        private async Task ExecuteAsync(IEnumerable<MigrationDefinition> migrations, MigrationContext context)
        {
            foreach (var definition in migrations.OrderBy(x => x.Version))
            {
                await definition.Migration.ExecuteAsync(context);
            }
        }

        private ExecutedMigrationCollection AddExecutedMigrations(ExecutedMigrationCollection executedMigrations, IEnumerable<MigrationDefinition> migrationsToExecute)
        {
            var newlyExecutedMigrations = migrationsToExecute
                    .GroupBy(x => x.Version)
                    .Select(x => new ExecutedMigration(x.Key, DateTime.Now))
                    .ToList();
            return executedMigrations.MergeWith(newlyExecutedMigrations);
        }
    }
}
