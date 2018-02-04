using AzureStorage.Migrations.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

        public IEnumerable<MigrationDefinition> FindMigrations()
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

            var results = candidates
                .Except(misconfigured)
                .Select(x => CreateDefinition(x.migrationAttribute, x.type))
                .ToList();

            AssertNoDuplicates(results);
            return results;
        }

        private MigrationDefinition CreateDefinition(MigrationAttribute attribute, Type @type)
        {
            var migration = migrationFactory.Create(type);
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
            if (duplicates.Count == 0)
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
