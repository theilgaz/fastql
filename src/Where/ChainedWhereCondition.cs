namespace Fastql.Where
{
    public class ChainedWhereCondition
    {
        private readonly string _prefix;
        private readonly string _connector;

        internal ChainedWhereCondition(string prefix, string connector)
        {
            _prefix = prefix;
            _connector = connector;
        }

        public WhereCondition Column(string columnName)
        {
            return new PrefixedWhereCondition(columnName, _prefix, _connector);
        }

        private class PrefixedWhereCondition : WhereCondition
        {
            private readonly string _prefix;
            private readonly string _connector;

            internal PrefixedWhereCondition(string columnName, string prefix, string connector) : base(columnName)
            {
                _prefix = prefix;
                _connector = connector;
            }

            protected override WhereExpression CreateExpression(string clause)
            {
                return new WhereExpression($"{_prefix} {_connector} {clause}");
            }
        }
    }
}
