using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace AzureStorage.Migrations.Runner.Storage
{
    public class ExecutedMigrationCollection: IEnumerable<ExecutedMigration>
    {
        private readonly List<ExecutedMigration> migrations;

        public ExecutedMigrationCollection()
            : this(Enumerable.Empty<ExecutedMigration>())
        {
        }

        public ExecutedMigrationCollection(IEnumerable<ExecutedMigration> migrations)
        {
            if (migrations == null) { throw new ArgumentNullException(nameof(migrations)); }

            var duplicates = migrations
                .GroupBy(x => x.Version)
                .Where(x => x.Skip(1).Any())
                .Select(x => x.Key)
                .ToList();
            if (duplicates.Count > 0)
            {
                var versions = string.Join(", ", duplicates);
                throw new ArgumentException($"Migration of versions {versions} was already executed.", nameof(ExecutedMigrationCollection.migrations));
            }

            this.migrations = migrations.ToList();
        }

        public ExecutedMigrationCollection MergeWith(ExecutedMigration migration)
            => MergeWith(new[] { migration });

        public ExecutedMigrationCollection MergeWith(IEnumerable<ExecutedMigration> otherMigrations)
            => new ExecutedMigrationCollection(migrations.Concat(otherMigrations));
        
        public IEnumerator<ExecutedMigration> GetEnumerator()
            => migrations.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator()
            => GetEnumerator();
    }
}
