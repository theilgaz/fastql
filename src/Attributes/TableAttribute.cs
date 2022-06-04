using System;

namespace Fastql.Attributes
{
    public class TableAttribute : Attribute
    {
        public string TableName { get; set; }
        public string Schema { get; set; } = "dbo";

        public OutputName Output { get; set; } = OutputName.Default;
        
        public TableAttribute(string table, string schema = "dbo", OutputName output = OutputName.Default)
        {
            TableName = table;
            Schema = schema;
            Output = output;
        }

    }
    
    public enum OutputName
    {
        Default,
        TableAndSchema,
        OnlyTable,
    }
     
} 