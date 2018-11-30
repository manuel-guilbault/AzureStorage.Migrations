using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner.Storage
{
    public interface IExecutedMigrationsStorage
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="action"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException">If the lease cannot be acquired.</exception>
        Task RunWithLeaseAsync(
            Func<ILeasedExecutedMigrationsStorage, Task> action, 
            CancellationToken cancellationToken = default);
    }

    public interface ILeasedExecutedMigrationsStorage
    {
        Task<ExecutedMigrationCollection> ReadAsync(CancellationToken cancellationToken = default);
        Task WriteAsync(ExecutedMigrationCollection executedMigrations, CancellationToken cancellationToken = default);
    }
}
