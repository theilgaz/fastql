using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Fastql
{
    public class FastQueryBuilder
    {
        private readonly List<QueryBuilderObject> _objects = new List<QueryBuilderObject>();

        private readonly string _table;
        private readonly string _where;

        private string _identityColumn = "";

        public FastQueryBuilder(string table, string where = null)
        {
            _table = table;
            _where = where;
        }

        public void AddIdentityColumn(string identity)
        {
            _identityColumn = identity;
        }

        public void Add(string key, string name, object value)
        {
            var obj = new QueryBuilderObject(key, name, value);
            if (_objects.Contains(obj))
                throw new DuplicateNameException("This field was already declared");

            _objects.Add(obj);
        }

        public void AddCondition(string parameterName, string name, object value)
        {
            var obj = new QueryBuilderObject(parameterName, name, value);

            if (_objects.Contains(obj))
                throw new DuplicateNameException("This field was already declared");
        }

        public string InsertSql
        {
            get
            {
                if (_objects.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var fields = string.Join(", ", _objects.Select(x => x.Key));
                var values = string.Join(", @", _objects.Select(x => x.Name));
                return $"INSERT INTO {_table}({fields}) VALUES(@{values}) ";
            }
        }

        public string InsertReturnObjectSql
        {
            get
            {
                if (_objects.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var fields = string.Join(", ", _objects.Select(x => x.Key));
                var values = string.Join(", @", _objects.Select(x => x.Name));
                return $"INSERT INTO {_table}({fields}) OUTPUT inserted . * VALUES(@{values});";
            }
        }

        public string UpdateSql
        {
            get
            {
                if (string.IsNullOrEmpty(_where))
                    throw new Exception("Where clause not provided.");

                if (_objects.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var sb = new StringBuilder();
                foreach (var obj in _objects)
                {
                    sb.Append($"{obj.Key} = @{obj.Name}, ");
                }

                return $"UPDATE {_table} SET {sb.ToString().Substring(0, sb.Length - 2)} {_where};";
            }
        }

        public string SelectSql
        {
            get
            {
                if (string.IsNullOrEmpty(_where))
                    throw new Exception("Where clause not provided.");

                if (_objects.Count == 0)
                    throw new Exception("Input parameters not provided.");

                var sb = new StringBuilder();
                foreach (var obj in _objects)
                {
                    sb.Append($"{obj.Key} as {obj.Name}, ");
                }

                return $"SELECT {sb.ToString().Substring(0, sb.Length - 2)} FROM {_table} {_where};";
            }
        }

        public string ReturnStatement
        {
            get
            {
                
                if (_objects.Count == 0)
                    throw new Exception("Input parameters not provided.");
                
                var sb = new StringBuilder();
                
                if (!string.IsNullOrEmpty(_identityColumn))
                {
                    sb.Append($"{_identityColumn}, ");
                }
                
                foreach (var obj in _objects)
                { 
                    var name = obj.Name;
                    var index = name.IndexOf(':');
                    if (index >= 0)
                        name = name.Substring(0, index);
                    
                    sb.Append($"{obj.Key} as {name}, ");
                }
                
                return sb.ToString().Substring(0, sb.Length - 2);
            }
        }

        public string TableName
        {
            get { return _table; }
        }
    }
}