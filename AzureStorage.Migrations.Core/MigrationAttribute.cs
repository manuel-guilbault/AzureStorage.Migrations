using System;
using System.Linq;

namespace AzureStorage.Migrations.Core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
    public class MigrationAttribute: Attribute
    {
        public MigrationAttribute(int version)
        {
            Version = version;
        }

        public int Version { get; }
        public string Tags { get; set; }

        public string[] GetTags()
            => Tags?
                .Split(',')
                .Select(x => x.Trim().ToLowerInvariant())
                .Where(x => x != "")
                .ToArray()
                ?? new string[0];
    }
}
