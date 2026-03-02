using System;
using System.Collections.Generic;
using Fastql.Core;
using Fastql.Validation;

namespace Fastql
{
    /// <summary>
    /// A small and fast library for building SQL queries from entity classes
    /// in a better way than regular string concatenation.
    /// </summary>
    public class FastqlBuilder<TEntity> where TEntity : class, new()
    {
        private readonly TEntity _entity;
        private readonly DatabaseType _databaseType;

        public FastqlBuilder()
        {
            _entity = new TEntity();
            _databaseType = DatabaseType.SqlServer;
        }

        public FastqlBuilder(DatabaseType databaseType)
        {
            _entity = new TEntity();
            _databaseType = databaseType;
        }

        public string TableName()
        {
            return QueryGenerator.GenerateTableName<TEntity>();
        }

        public string InsertReturnObjectQuery()
        {
            return QueryGenerator.GenerateInsertQuery(_entity, _databaseType, false, true);
        }

        public string InsertQuery(bool returnIdentity = false)
        {
            return QueryGenerator.GenerateInsertQuery(_entity, _databaseType, returnIdentity, false);
        }

        public string InsertStatement(bool returnIdentity = false)
        {
            return QueryGenerator.GenerateInsertStatement<TEntity>(_databaseType, returnIdentity);
        }

        public string UpdateQuery(TEntity entity, string where)
        {
            return QueryGenerator.GenerateUpdateQuery(entity, where, _databaseType);
        }

        public string UpdateStatement(TEntity entity, string where)
        {
            return QueryGenerator.GenerateUpdateStatement<TEntity>(where);
        }

        public string DeleteQuery(string where)
        {
            return QueryGenerator.GenerateDeleteQuery<TEntity>(where);
        }

        public string SelectQuery(string where)
        {
            return QueryGenerator.GenerateSelectQuery<TEntity>(where);
        }

        /// <summary>
        /// Generates SELECT query with desired columns, where conditions and top records.
        /// </summary>
        /// <param name="columns">If its length greater than zero (0), it'll be included. Otherwise all columns will be fetched.</param>
        /// <param name="where"></param>
        /// <param name="top">Default 1000 rows will be fetched.</param>
        /// <returns></returns>
        public string SelectQuery(string[] columns, string where, int top = 1000)
        {
            return QueryGenerator.GenerateSelectQueryWithColumns<TEntity>(columns, where, top, _databaseType);
        }

        public ValidationResult Validate(TEntity entity)
        {
            return EntityValidator.Validate(entity);
        }

        public string BulkInsertQuery(IReadOnlyList<TEntity> entities)
        {
            return BulkQueryGenerator.BulkInsertQuery(entities, _databaseType);
        }

        public string BulkUpdateQuery(IReadOnlyList<TEntity> entities)
        {
            return BulkQueryGenerator.BulkUpdateQuery(entities, _databaseType);
        }

        public string UpsertQuery()
        {
            return UpsertQueryGenerator.UpsertQuery<TEntity>(_databaseType);
        }
    }
}
