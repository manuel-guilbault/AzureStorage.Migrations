using System;

namespace AzureStorage.Migrations.Storage
{
    public class ExecutedMigration
    {
        public ExecutedMigration(int version, DateTime executedAt)
        {
            Version = version;
            ExecutedAt = executedAt;
        }

        public int Version { get; }
        public DateTime ExecutedAt { get; }
    }
}
