using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace AzureStorage.Migrations.Console
{
    public class SettingsParser
    {
        private const string assemblySwitch = "-a";
        private const string connectionStringSwitch = "-cs";
        private const string containerSwitch = "-c";
        private const string blobSwitch = "-b";
        private const string tagsSwitch = "-t";
        private const string propertiesSwitch = "-p";

        private Stack<string> tokens;
        private string assembly;
        private string connectionString;
        private string container;
        private string blob;
        private List<string> tags;
        private IDictionary<string, string> properties;

        public Settings Parse(string[] values)
        {
            tokens = new Stack<string>(values.Reverse());
            assembly = null;
            connectionString = null;
            container = "migrations";
            blob = "default";
            tags = new List<string>();
            properties = new Dictionary<string, string>();

            while (tokens.Count > 0)
            {
                var @switch = tokens.Pop();
                switch (@switch)
                {
                    case assemblySwitch:
                        ReadAssembly();
                        break;

                    case connectionStringSwitch:
                        ReadConnectionString();
                        break;

                    case containerSwitch:
                        ReadContainer();
                        break;

                    case blobSwitch:
                        ReadBlob();
                        break;

                    case tagsSwitch:
                        ReadTags();
                        break;

                    case propertiesSwitch:
                        ReadProperties();
                        break;

                    default:
                        throw new Exception($"Unknown argument {@switch}.");
                }
            }

            if (assembly == null) { throw new Exception($"The {assemblySwitch} argument is required."); }
            if (connectionString == null) { throw new Exception($"The {connectionStringSwitch} argument is required."); }
            if (container == null) { throw new Exception($"The {containerSwitch} argument is required."); }
            if (blob == null) { throw new Exception($"The {blobSwitch} argument is required."); }

            return new Settings(assembly, connectionString, container, blob, tags.ToArray(), properties);
        }

        private void ReadAssembly()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {assemblySwitch} argument must be followed by the name of the assembly containing the migration classes.");
            }

            assembly = tokens.Pop();
        }

        private void ReadConnectionString()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {connectionStringSwitch} argument must be followed by the Azure Storage account connection string.");
            }

            connectionString = tokens.Pop();
        }

        private void ReadContainer()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {containerSwitch} argument must be followed by the name of the Azure Blob container where the executed migrations are stored.");
            }

            container = tokens.Pop();
        }

        private void ReadBlob()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {blobSwitch} argument must be followed by the name of the Azure Blob where the executed migrations are stored.");
            }

            blob = tokens.Pop();
        }

        private void ReadTags()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {tagsSwitch} argument must be followed by one or more tags.");
            }
            
            while (tokens.Count > 0 && !tokens.Peek().StartsWith("-"))
            {
                tags.Add(tokens.Pop());
            }
        }

        private void ReadProperties()
        {
            if (tokens.Count == 0)
            {
                throw new Exception($"The {propertiesSwitch} argument must be followed by one or more property definition.");
            }

            while (tokens.Count > 0 && !tokens.Peek().StartsWith("-"))
            {
                var raw = tokens.Pop();
                var parts = raw.Split(new[] { '=' }, 2);
                if (parts.Length < 2)
                {
                    throw new Exception($"Properties must have the following format: 'name=value'.");
                }

                properties.Add(parts[0], parts[1]);
            }
        }
    }
}
