namespace Fastql.Where
{
    public class WhereExpression
    {
        private readonly string _expression;

        internal WhereExpression(string expression)
        {
            _expression = expression;
        }

        public ChainedWhereCondition And => new ChainedWhereCondition(_expression, "AND");
        public ChainedWhereCondition Or => new ChainedWhereCondition(_expression, "OR");

        public string Build()
        {
            return _expression;
        }

        public static implicit operator string(WhereExpression expression)
        {
            return expression.Build();
        }

        public override string ToString()
        {
            return _expression;
        }
    }
}
