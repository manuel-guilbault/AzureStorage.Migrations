using System.Collections.Generic;

namespace AzureStorage.Migrations.Runtime
{
    public interface IMigrationFinder
    {
        IEnumerable<MigrationDefinition> FindMigrations();
    }
}