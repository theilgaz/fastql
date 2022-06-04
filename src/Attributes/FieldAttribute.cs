using System;

namespace Fastql.Attributes
{
    public class FieldAttribute : Attribute
    {
        public string FieldName { get; set; }

        public FieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}