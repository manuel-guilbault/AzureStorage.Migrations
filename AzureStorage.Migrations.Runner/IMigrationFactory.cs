using AzureStorage.Migrations.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner
{
    public interface IMigrationFactory
    {
        Task<IMigration> CreateAsync(Type @type, CancellationToken cancellationToken = default);
    }
}
