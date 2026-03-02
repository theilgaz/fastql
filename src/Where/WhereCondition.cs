namespace Fastql.Where
{
    public class WhereCondition
    {
        private readonly string _columnName;

        internal WhereCondition(string columnName)
        {
            _columnName = columnName;
        }

        protected virtual WhereExpression CreateExpression(string clause)
        {
            return new WhereExpression(clause);
        }

        public WhereExpression Equals(string value)
        {
            return CreateExpression($"{_columnName} = {value}");
        }

        public WhereExpression NotEquals(string value)
        {
            return CreateExpression($"{_columnName} <> {value}");
        }

        public WhereExpression GreaterThan(string value)
        {
            return CreateExpression($"{_columnName} > {value}");
        }

        public WhereExpression LessThan(string value)
        {
            return CreateExpression($"{_columnName} < {value}");
        }

        public WhereExpression GreaterThanOrEqual(string value)
        {
            return CreateExpression($"{_columnName} >= {value}");
        }

        public WhereExpression LessThanOrEqual(string value)
        {
            return CreateExpression($"{_columnName} <= {value}");
        }

        public WhereExpression Like(string value)
        {
            return CreateExpression($"{_columnName} LIKE {value}");
        }

        public WhereExpression In(string value)
        {
            return CreateExpression($"{_columnName} IN ({value})");
        }

        public WhereExpression IsNull()
        {
            return CreateExpression($"{_columnName} IS NULL");
        }

        public WhereExpression IsNotNull()
        {
            return CreateExpression($"{_columnName} IS NOT NULL");
        }

        public WhereExpression Between(string low, string high)
        {
            return CreateExpression($"{_columnName} BETWEEN {low} AND {high}");
        }
    }
}
