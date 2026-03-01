using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Fastql.Caching
{
    /// <summary>
    /// Cached metadata for an entity type, including table info and pre-computed property lists.
    /// </summary>
    public sealed class EntityMetadata
    {
        public Type EntityType { get; }
        public string TableName { get; }
        public string Schema { get; }
        public OutputName OutputFormat { get; }
        public IReadOnlyList<PropertyMetadata> Properties { get; }
        public IReadOnlyList<PropertyMetadata> InsertableProperties { get; }
        public IReadOnlyList<PropertyMetadata> UpdatableProperties { get; }
        public IReadOnlyList<PropertyMetadata> SelectableProperties { get; }
        public PropertyMetadata? PrimaryKeyProperty { get; }

        public EntityMetadata(Type entityType)
        {
            EntityType = entityType;

            // Get table attribute
            var tableAttr = entityType.GetCustomAttribute<TableAttribute>();
            if (tableAttr != null)
            {
                TableName = tableAttr.TableName;
                Schema = tableAttr.Schema;
                OutputFormat = tableAttr.Output;
            }
            else
            {
                TableName = entityType.Name;
                Schema = "dbo";
                OutputFormat = OutputName.Default;
            }

            // Cache all property metadata
            var properties = entityType.GetProperties(BindingFlags.Public | BindingFlags.Instance)
                .Select(p => new PropertyMetadata(p))
                .ToList();

            Properties = properties;
            InsertableProperties = properties.Where(p => p.IsInsertable).ToList();
            UpdatableProperties = properties.Where(p => p.IsUpdatable).ToList();
            SelectableProperties = properties.Where(p => p.IsSelectable).ToList();
            PrimaryKeyProperty = properties.FirstOrDefault(p => p.IsPrimaryKey);
        }

        /// <summary>
        /// Gets the formatted table name based on OutputFormat setting.
        /// </summary>
        public string GetFormattedTableName()
        {
            return OutputFormat switch
            {
                OutputName.OnlyTable => TableName,
                OutputName.TableAndSchema => Schema + "." + TableName,
                _ => $"[{Schema}].[{TableName}]"
            };
        }
    }
}
