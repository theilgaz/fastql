using System;

namespace Fastql.Attributes
{
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string Schema { get; set; } = "dbo";

        public TableOutputName TableOutputName { get; set; } = TableOutputName.Default;
        
        public TableAttribute(string table, string schema = "dbo", TableOutputName tableOutputName = TableOutputName.Default)
        {
            TableName = table;
            Schema = schema;
            TableOutputName = tableOutputName;
        }

    }
    
    public enum TableOutputName
    {
        Default,
        TableAndSchema,
        OnlyTable,
    }
     
} 