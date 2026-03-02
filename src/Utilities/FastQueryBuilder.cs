using System;
using System.Collections.Generic;
using System.Linq;
using Fastql.Exceptions;

namespace Fastql
{
    public class FastQueryBuilder
    {
        private readonly List<QueryBuilderObject> _objects = new List<QueryBuilderObject>();

        private readonly string _table;
        private readonly string? _where;

        private string _identityColumn = "";

        public FastQueryBuilder(string table, string? where = null)
        {
            _table = table;
            _where = where;
        }

        public void AddIdentityColumn(string identity)
        {
            _identityColumn = identity;
        }

        public void Add(string key, string name, object? value)
        {
            var obj = new QueryBuilderObject(key, name, value);
            if (_objects.Contains(obj))
                throw new DuplicateFieldException(key);

            _objects.Add(obj);
        }

        public string InsertSql
        {
            get
            {
                if (_objects.Count == 0)
                    throw new MissingParametersException();

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
                    throw new MissingParametersException();

                var fields = string.Join(", ", _objects.Select(x => x.Key));
                var values = string.Join(", @", _objects.Select(x => x.Name));
                return $"INSERT INTO {_table}({fields}) OUTPUT inserted.* VALUES(@{values});";
            }
        }

        public string UpdateSql
        {
            get
            {
                if (string.IsNullOrEmpty(_where))
                    throw new MissingWhereClauseException();

                if (_objects.Count == 0)
                    throw new MissingParametersException();

                var setClauses = string.Join(", ", _objects.Select(obj => $"{obj.Key} = @{obj.Name}"));
                return $"UPDATE {_table} SET {setClauses} {_where};";
            }
        }

        public string SelectSql
        {
            get
            {
                if (string.IsNullOrEmpty(_where))
                    throw new MissingWhereClauseException();

                if (_objects.Count == 0)
                    throw new MissingParametersException();

                var columnList = string.Join(", ", _objects.Select(obj => $"{obj.Key} as {obj.Name}"));
                return $"SELECT {columnList} FROM {_table} {_where};";
            }
        }

        public string ReturnStatement
        {
            get
            {
                if (_objects.Count == 0)
                    throw new MissingParametersException();

                var parts = new List<string>();

                if (!string.IsNullOrEmpty(_identityColumn))
                {
                    parts.Add(_identityColumn);
                }

                foreach (var obj in _objects)
                {
                    var name = obj.Name;
                    var index = name.IndexOf(':');
                    if (index >= 0)
                        name = name.Substring(0, index);

                    parts.Add($"{obj.Key} as {name}");
                }

                return string.Join(", ", parts);
            }
        }

        public string TableName
        {
            get { return _table; }
        }
    }
}
