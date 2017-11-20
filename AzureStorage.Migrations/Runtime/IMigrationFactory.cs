using AzureStorage.Migrations.Core;
using System;

namespace AzureStorage.Migrations.Runtime
{
    public interface IMigrationFactory
    {
        IMigration Create(Type @type);
    }
}
