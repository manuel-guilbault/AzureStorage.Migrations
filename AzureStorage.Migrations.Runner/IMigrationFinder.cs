using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner
{
    public interface IMigrationFinder
    {
        Task<IEnumerable<MigrationDefinition>> FindMigrationsAsync(CancellationToken cancellationToken = default);
    }
}