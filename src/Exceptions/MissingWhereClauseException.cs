namespace Fastql.Exceptions
{
    public class MissingWhereClauseException : FastqlException
    {
        public MissingWhereClauseException()
            : base("WHERE clause is required for this operation.") { }

        public MissingWhereClauseException(string message) : base(message) { }
    }
}
