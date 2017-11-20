using System;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Storage
{
    public interface IExecutedMigrationsStorage
    {
        Task<IDisposable> LockAsync();
        Task<ExecutedMigrationCollection> ReadAsync();
        Task WriteAsync(ExecutedMigrationCollection executedMigrations);
    }
}
