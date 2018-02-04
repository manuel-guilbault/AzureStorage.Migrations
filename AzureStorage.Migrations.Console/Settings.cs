using System;
using System.Collections.Generic;

namespace AzureStorage.Migrations.Console
{
    public class Settings
    {
        public Settings(
            string assembly,
            string connectionString,
            string container,
            string blob,
            string[] tags,
            IDictionary<string, string> properties)
        {
            Assembly = assembly ?? throw new ArgumentNullException(nameof(assembly));
            ConnectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
            Container = container ?? throw new ArgumentNullException(nameof(container));
            Blob = blob ?? throw new ArgumentNullException(nameof(blob));
            Tags = tags ?? throw new ArgumentNullException(nameof(tags));
            Properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string Assembly { get; }
        public string ConnectionString { get; }
        public string Container { get; }
        public string Blob { get; }
        public string[] Tags { get; }
        public IDictionary<string, string> Properties { get; }
    }
}
