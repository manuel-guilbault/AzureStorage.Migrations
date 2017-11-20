using System;

namespace AzureStorage.Migrations
{
    public class Settings
    {
        public Settings(
            string assembly,
            string connectionString,
            string container,
            string blob,
            string[] tags)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Blob = blob ?? throw new ArgumentNullException(nameof(blob));
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
        }

        public string Assembly { get; }
        public string ConnectionString { get; }
        public string Container { get; }
        public string Blob { get; }
        public string[] Tags { get; }
    }
}
