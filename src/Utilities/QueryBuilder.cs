using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Fastql.Utilities
{
    public class QueryBuilder
    {
        private readonly Dictionary<string, object> _params = new Dictionary<string, object>();

        private readonly string _table;
        private readonly string _where;

        private string _identityColumn = "";

        public QueryBuilder(string table, string where = null)
        {
            _table = table;
            _where = where;
        }

        public void AddIdentityColumn(string identity)
        {
            _identityColumn = identity;
        }

        public void Add(string parameter, object value)
        {
            if (_params.ContainsKey(parameter))
                throw new DuplicateNameException("This field was already declared");

            _params.Add(parameter, value);
        }

        public void AddCondition(string parameterName, object value)
        {
            if (_params.ContainsKey(parameterName))
                throw new DuplicateNameException("This field was already declared");
        }

        public string InsertSql
        {
            get
            {
                if (_params.Keys.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var fields = string.Join(", ", _params.Keys);
                var values = string.Join(", @", _params.Keys);
                return $"INSERT INTO {_table}({fields}) VALUES(@{values});";
            }
        }

        public string UpdateSql
        {
            get
            {
                if (string.IsNullOrEmpty(_where))
                    throw new Exception("Where clause not provided.");

                if (_params.Keys.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var sb = new StringBuilder();
                foreach (var parameterName in _params.Keys)
                {
                    sb.Append($"{parameterName} = @{parameterName}, ");
                }

                return $"UPDATE {_table} SET {sb.ToString().Substring(0, sb.Length - 2)} {_where};";
            }
        }

        public string TableName
        {
            get { return _table; }
        }
    }
}