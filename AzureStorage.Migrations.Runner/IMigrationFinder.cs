using System.Collections.Generic;

namespace AzureStorage.Migrations.Runner
{
    public interface IMigrationFinder
    {
        IEnumerable<MigrationDefinition> FindMigrations();
    }
}