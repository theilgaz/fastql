using System;
using System.Linq;
using Fastql.Core;

namespace Fastql
{
    public static class FastqlHelper<TEntity> where TEntity : class, new()
    {
        private static DatabaseType _databaseType = DatabaseType.SqlServer;

        public static DatabaseType DatabaseType
        {
            get => _databaseType;
            set => _databaseType = value;
        }

        public static DatabaseType SetDatabaseType(DatabaseType databaseType)
        {
            _databaseType = databaseType;
            return _databaseType;
        }

        public static DatabaseType GetDatabaseType()
        {
            return _databaseType;
        }

        static string TableName()
        {
            return QueryGenerator.GenerateTableName<TEntity>();
        }

        public static string InsertReturnObjectQuery()
        {
            var entity = new TEntity();
            return QueryGenerator.GenerateInsertQuery(entity, _databaseType, false, true);
        }

        public static string InsertQuery(bool returnIdentity = false)
        {
            var entity = new TEntity();
            return QueryGenerator.GenerateInsertQuery(entity, _databaseType, returnIdentity, false);
        }

        public static string InsertStatement(bool returnIdentity = false)
        {
            return QueryGenerator.GenerateInsertStatement<TEntity>(_databaseType, returnIdentity);
        }

        public static string SelectQuery(string where)
        {
            return $"SELECT * FROM {TableName()} WHERE {where};";
        }

        public static string SelectQuery(string[] columns, string where, int top = 1000)
        {
            var tableName = TableName();
            var s = $"[{string.Join("],[", columns.Select(i => i.Replace("[", "")))}]";
            return columns is { Length: > 0 }
                ? $"SELECT TOP({top}) {string.Join(",", s)} FROM {tableName} WHERE {where};"
                : $"SELECT TOP({top}) * FROM {tableName} WHERE {where};";
        }

        public static string UpdateQuery(TEntity entity, string where)
        {
            return QueryGenerator.GenerateUpdateQuery(entity, where, _databaseType);
        }

        public static string UpdateStatement(TEntity entity, string where)
        {
            return QueryGenerator.GenerateUpdateStatement<TEntity>(where);
        }

        public static string DeleteQuery(string where)
        {
            return QueryGenerator.GenerateDeleteQuery<TEntity>(where);
        }
    }
}
