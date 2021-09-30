using System;

namespace Fastql.Attributes
{
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string Schema { get; set; } = "dbo";

        public TableAttribute(string table, string schema = "dbo")
        {
            TableName = table;
            Schema = schema;
        }

    }
}
