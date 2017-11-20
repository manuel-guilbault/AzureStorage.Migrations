using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using Microsoft.WindowsAzure.Storage;

namespace AzureStorage.Migrations.Core
{
    public class TableRow : ITableEntity
    {
        private IDictionary<string, EntityProperty> properties;

        public TableRow()
        {
            properties = new Dictionary<string, EntityProperty>();
        }

        public TableRow(string partitionKey, string rowKey)
            : this(partitionKey, rowKey, new Dictionary<string, EntityProperty>())
        {
        }

        public TableRow(string partitionKey, string rowKey, IDictionary<string, EntityProperty> properties)
        {
            PartitionKey = partitionKey ?? throw new ArgumentNullException(nameof(partitionKey));
            RowKey = rowKey ?? throw new ArgumentNullException(nameof(rowKey));
            this.properties = properties ?? throw new ArgumentNullException(nameof(properties));
        }

        public string PartitionKey { get; set; }
        public string RowKey { get; set; }
        public DateTimeOffset Timestamp { get; set; }
        public string ETag { get; set; }

        public bool HasProperty(string name)
            => properties.ContainsKey(name);

        public EntityProperty GetProperty(string name)
            => properties[name];

        public void SetProperty(string name, EntityProperty value)
            => properties[name] = value;

        public void RemoveProperty(string name)
            => properties.Remove(name);

        public void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
            => this.properties = properties;

        public IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
            => properties;
    }
}
