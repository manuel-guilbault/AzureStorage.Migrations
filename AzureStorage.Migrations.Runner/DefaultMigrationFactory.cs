using AzureStorage.Migrations.Core;
using System;

namespace AzureStorage.Migrations.Runner
{
    public class DefaultMigrationFactory : IMigrationFactory
    {
        public IMigration Create(Type type)
        {
            if (type == null) { throw new ArgumentNullException(nameof(type)); }
            if (!typeof(IMigration).IsAssignableFrom(type)) { throw new ArgumentException($"{nameof(type)} must be a subclass of {nameof(IMigration)}.", nameof(type)); }

            return (IMigration)Activator.CreateInstance(type);
        }
    }
}
