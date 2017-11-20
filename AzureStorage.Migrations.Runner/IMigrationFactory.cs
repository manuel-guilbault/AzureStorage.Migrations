using AzureStorage.Migrations.Core;
using System;

namespace AzureStorage.Migrations.Runner
{
    public interface IMigrationFactory
    {
        IMigration Create(Type @type);
    }
}
