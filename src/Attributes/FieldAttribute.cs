using System;
using Fastql.Utilities;

namespace Fastql.Attributes
{
    public class FieldAttribute : Attribute
    {
        public string FieldName { get; set; }

        public FieldType FieldType { get; set; }

        public FieldAttribute(string fieldName)
        {
            FieldName = fieldName;
            FieldType = FieldType.Initial;
        }

        public FieldAttribute(string fieldName, FieldType fieldType)
        {
            FieldName = fieldName;
            FieldType = fieldType;
        }
    }

}