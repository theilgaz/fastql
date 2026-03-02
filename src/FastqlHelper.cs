using System;
using System.Collections.Generic;
using Fastql.Core;
using Fastql.Validation;

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

        [Obsolete("Use the DatabaseType property instead.")]
        public static DatabaseType SetDatabaseType(DatabaseType databaseType)
        {
            _databaseType = databaseType;
            return _databaseType;
        }

        [Obsolete("Use the DatabaseType property instead.")]
        public static DatabaseType GetDatabaseType()
        {
            return _databaseType;
        }

        public static string TableName()
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
            return QueryGenerator.GenerateSelectQuery<TEntity>(where);
        }

        public static string SelectQuery(string[] columns, string where, int top = 1000)
        {
            return QueryGenerator.GenerateSelectQueryWithColumns<TEntity>(columns, where, top, _databaseType);
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

        public static ValidationResult Validate(TEntity entity)
        {
            return EntityValidator.Validate(entity);
        }

        public static string BulkInsertQuery(IReadOnlyList<TEntity> entities)
        {
            return BulkQueryGenerator.BulkInsertQuery(entities, _databaseType);
        }

        public static string BulkUpdateQuery(IReadOnlyList<TEntity> entities)
        {
            return BulkQueryGenerator.BulkUpdateQuery(entities, _databaseType);
        }

        public static string UpsertQuery()
        {
            return UpsertQueryGenerator.UpsertQuery<TEntity>(_databaseType);
        }
    }
}
