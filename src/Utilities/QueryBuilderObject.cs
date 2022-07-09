namespace Fastql
{
    public class QueryBuilderObject
    {
        public string Key { get; set; }
        public string Name { get; set; }
        public object Value { get; set; }

        public QueryBuilderObject()
        {
        }

        public QueryBuilderObject(string key, string name, object value)
        {
            Key = key;
            Name = name;
            Value = value;
        }
    }
}