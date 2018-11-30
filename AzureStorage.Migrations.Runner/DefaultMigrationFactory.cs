using AzureStorage.Migrations.Core;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace AzureStorage.Migrations.Runner
{
    public class DefaultMigrationFactory : IMigrationFactory
    {
        public Task<IMigration> CreateAsync(Type type, CancellationToken cancellationToken = default)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (!typeof(IMigration).IsAssignableFrom(type)) { throw new ArgumentException($"{nameof(type)} must be a subclass of {nameof(IMigration)}.", nameof(type)); }

            var migration = (IMigration)Activator.CreateInstance(type);
            return Task.FromResult(migration);
        }
    }
}
