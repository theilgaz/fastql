namespace Fastql.Validation
{
    public sealed class ValidationError
    {
        public string PropertyName { get; }
        public string Message { get; }

        public ValidationError(string propertyName, string message)
        {
            PropertyName = propertyName;
            Message = message;
        }
    }
}
