using NFluent;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AzureStorage.Migrations.Console.Tests
{
    public class SettingsParserTests
    {
        [Fact]
        public void Should_Parse()
        {
            var assembly = "c:/some/assembly/path.dll";
            var connectionString = "some_connection_string";
            var container = "my-container";
            var blob = "my-blob";
            var tags = new[] { "tag1", "tag2" };
            var properties = new Dictionary<string, string>()
            {
                ["property1"] = "value1",
                ["property2"] = "value2",
            };

            var command = $"-a {assembly} -cs {connectionString} -c {container} -b {blob} -t {string.Join(" ", tags)} -p {string.Join(" ", properties.Select(x => $"{x.Key}={x.Value}"))}";
            
            var result = SettingsParser.Parse(command.Split(' '));

            Check.That(result.Assembly).Equals(assembly);
            Check.That(result.ConnectionString).Equals(connectionString);
            Check.That(result.Container).Equals(container);
            Check.That(result.Blob).Equals(blob);
            Check.That(result.Tags).ContainsExactly(tags);
            Check.That(result.Properties).ContainsExactly(properties);
        }
    }
}
