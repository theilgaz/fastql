namespace Fastql.Where
{
    public static class Where
    {
        public static WhereCondition Column(string columnName)
        {
            return new WhereCondition(columnName);
        }
    }
}
