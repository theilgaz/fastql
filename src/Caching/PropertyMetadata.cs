using System;
using System.Reflection;

namespace Fastql.Caching
{
    /// <summary>
    /// Cached metadata for a single property, avoiding repeated reflection calls.
    /// </summary>
    public sealed class PropertyMetadata
    {
        public PropertyInfo PropertyInfo { get; }
        public string PropertyName { get; }
        public string ColumnName { get; }
        public FieldType FieldType { get; }
        public bool IsPrimaryKey { get; }
        public bool IsInsertable { get; }
        public bool IsUpdatable { get; }
        public bool IsSelectable { get; }
        public bool IsCustomField { get; }
        public bool IsIgnored { get; }
        public int? MaxLength { get; }
        public object? DefaultValue { get; }
        public bool IsRequired { get; }

        public PropertyMetadata(PropertyInfo propertyInfo)
        {
            PropertyInfo = propertyInfo;
            PropertyName = propertyInfo.Name;

            // Check for FieldAttribute or ColumnAttribute (alias)
            var fieldAttr = propertyInfo.GetCustomAttribute<FieldAttribute>();
            var columnAttr = propertyInfo.GetCustomAttribute<ColumnAttribute>();

            if (fieldAttr != null)
            {
                ColumnName = fieldAttr.FieldName;
                FieldType = fieldAttr.FieldType;
            }
            else if (columnAttr != null)
            {
                ColumnName = columnAttr.ColumnName;
                FieldType = columnAttr.FieldType;
            }
            else
            {
                ColumnName = propertyInfo.Name;
                FieldType = FieldType.Initial;
            }

            // Cache attribute checks
            IsPrimaryKey = Attribute.IsDefined(propertyInfo, typeof(IsPrimaryKeyAttribute)) ||
                           Attribute.IsDefined(propertyInfo, typeof(PKAttribute));

            IsCustomField = Attribute.IsDefined(propertyInfo, typeof(CustomFieldAttribute));
            IsIgnored = Attribute.IsDefined(propertyInfo, typeof(IgnoreAttribute));

            var isNotInsertable = Attribute.IsDefined(propertyInfo, typeof(IsNotInsertableAttribute));
            var isSelectOnly = Attribute.IsDefined(propertyInfo, typeof(SelectOnlyAttribute));

            IsInsertable = !IsPrimaryKey && !isNotInsertable && !isSelectOnly && !IsCustomField && !IsIgnored;

            var isNotUpdatable = Attribute.IsDefined(propertyInfo, typeof(IsNotUpdatableAttribute));
            IsUpdatable = !IsPrimaryKey && !isNotUpdatable && !isSelectOnly && !IsCustomField && !IsIgnored;

            IsSelectable = !IsCustomField && !IsIgnored;

            // New attribute metadata
            var maxLengthAttr = propertyInfo.GetCustomAttribute<MaxLengthAttribute>();
            MaxLength = maxLengthAttr?.Length;

            var defaultValueAttr = propertyInfo.GetCustomAttribute<DefaultValueAttribute>();
            DefaultValue = defaultValueAttr?.Value;

            IsRequired = Attribute.IsDefined(propertyInfo, typeof(RequiredAttribute));
        }

        /// <summary>
        /// Gets the parameter name with optional type cast suffix for Postgres.
        /// </summary>
        public string GetParameterName(bool includeTypeCast)
        {
            if (!includeTypeCast)
                return PropertyName;

            return FieldType switch
            {
                FieldType.Jsonb => PropertyName + "::jsonb",
                FieldType.Timestamp => PropertyName + "::timestamp",
                FieldType.Date => PropertyName + "::date",
                FieldType.Time => PropertyName + "::time",
                _ => PropertyName
            };
        }

        /// <summary>
        /// Gets the value of this property from the given entity instance.
        /// </summary>
        public object? GetValue(object entity)
        {
            return PropertyInfo.GetValue(entity);
        }
    }
}
