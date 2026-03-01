namespace Fastql
{
    public class QueryBuilderObject
    {
        public string Key { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }

        public QueryBuilderObject()
        {
        }

        public QueryBuilderObject(string key, string name, object? value)
        {
            Key = key;
            Name = name;
            Value = value;
        }
    }
}