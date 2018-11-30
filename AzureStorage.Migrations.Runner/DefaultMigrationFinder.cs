using AzureStorage.Migrations.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner
{
    public class DefaultMigrationFinder: IMigrationFinder
    {
        private readonly IMigrationFactory migrationFactory;
        private readonly Assembly[] assemblies;

        public DefaultMigrationFinder(IMigrationFactory migrationFactory, params Assembly[] assemblies)
        {
            this.migrationFactory = migrationFactory ?? throw new ArgumentNullException(nameof(migrationFactory));
            this.assemblies = assemblies ?? throw new ArgumentNullException(nameof(assemblies));
        }

        public async Task<IEnumerable<MigrationDefinition>> FindMigrationsAsync(CancellationToken cancellationToken = default)
        {
            var candidates = (
                from assembly in assemblies
                from type in assembly.GetExportedTypes()
                let migrationAttribute = type.GetCustomAttribute<MigrationAttribute>()
                where migrationAttribute != null
                select new { migrationAttribute, type }
            ).ToList();

            var misconfigured = candidates
                .Where(x => !typeof(IMigration).IsAssignableFrom(x.type))
                .ToList();
            //TODO warn misconfigured

            var results = await Task.WhenAll(
                candidates
                    .Except(misconfigured)
                    .Select(x => CreateDefinitionAsync(x.migrationAttribute, x.type, cancellationToken)));

            AssertNoDuplicates(results);
            return results;
        }

        private async Task<MigrationDefinition> CreateDefinitionAsync(
            MigrationAttribute attribute, 
            Type @type,
            CancellationToken cancellationToken)
        {
            var migration = await migrationFactory.CreateAsync(type, cancellationToken);
            return new MigrationDefinition(
                attribute.Version,
                attribute.GetTags(),
                migration);
        }

        private void AssertNoDuplicates(IEnumerable<MigrationDefinition> migrations)
        {
            var duplicates = migrations
                .GroupBy(x => x.Version)
                .Where(x => x.Skip(1).Any())
                .ToList();
            if (duplicates.Count > 0)
            {
                var message = "Multiple migrations found with the same version number:\n"
                    + string.Join("\n", duplicates.Select(d => formatDuplicates(d.Key, d)));
                throw new MigrationException(message);


                string formatDuplicates(int version, IEnumerable<MigrationDefinition> definitions)
                    => $"  Version {version}:\n{string.Join("\n", definitions.Select(formatDuplicate))}";

                string formatDuplicate(MigrationDefinition definition)
                    => $"    {definition.Migration.GetType()}";
            }
        }
    }
}
