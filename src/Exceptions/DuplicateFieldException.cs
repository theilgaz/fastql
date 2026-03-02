namespace Fastql.Exceptions
{
    public class DuplicateFieldException : FastqlException
    {
        public string FieldName { get; }

        public DuplicateFieldException(string fieldName)
            : base($"The field '{fieldName}' was already declared.")
        {
            FieldName = fieldName;
        }
    }
}
