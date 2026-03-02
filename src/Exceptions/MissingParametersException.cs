namespace Fastql.Exceptions
{
    public class MissingParametersException : FastqlException
    {
        public MissingParametersException()
            : base("No properties to include in query.") { }

        public MissingParametersException(string message) : base(message) { }
    }
}
