using AzureStorage.Migrations.Runner.Storage;
using System.Collections.Generic;
using System.Linq;

namespace AzureStorage.Migrations.Runner
{
    public static class MigrationDefinitionEnumerableExtensions
    {
        public static IReadOnlyList<MigrationDefinition> FindMigrationsToExecute(
            this IEnumerable<MigrationDefinition> migrationDefinitions,
            ExecutedMigrationCollection executedMigrations,
            params string[] tags)
            => migrationDefinitions
                .Where(m => !executedMigrations.Any(em => em.Version == m.Version))
                .Where(m => m.Matches(tags))
                .ToList();
    }
}
