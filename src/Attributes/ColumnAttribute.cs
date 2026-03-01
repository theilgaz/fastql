using System;

namespace Fastql
{
    /// <summary>
    /// Maps a property to a database column with a different name.
    /// This is an alias for FieldAttribute, providing EF Core familiarity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ColumnAttribute : Attribute
    {
        public string ColumnName { get; set; }

        public FieldType FieldType { get; set; }

        public ColumnAttribute(string columnName)
        {
            ColumnName = columnName;
            FieldType = FieldType.Initial;
        }

        public ColumnAttribute(string columnName, FieldType fieldType)
        {
            ColumnName = columnName;
            FieldType = fieldType;
        }
    }
}
