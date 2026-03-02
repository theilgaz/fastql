using System;
using System.Linq;
using System.Text;
using Fastql.Caching;

namespace Fastql.Core
{
    /// <summary>
    /// Internal query generator using cached metadata for all reflection operations.
    /// </summary>
    internal static class QueryGenerator
    {
        /// <summary>
        /// Generates the formatted table name for an entity type.
        /// </summary>
        public static string GenerateTableName<TEntity>() where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            return metadata.GetFormattedTableName();
        }

        /// <summary>
        /// Generates an INSERT query with actual values from the entity.
        /// </summary>
        public static string GenerateInsertQuery<TEntity>(
            TEntity entity,
            DatabaseType dbType,
            bool returnIdentity,
            bool returnObject) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var qb = new FastQueryBuilder(tableName);

            // Add identity column if present
            if (metadata.PrimaryKeyProperty != null)
            {
                qb.AddIdentityColumn(metadata.PrimaryKeyProperty.PropertyName);
            }

            // Add insertable properties
            foreach (var prop in metadata.InsertableProperties)
            {
                var paramName = prop.GetParameterName(dbType == DatabaseType.Postgres);
                qb.Add(prop.ColumnName, paramName, prop.GetValue(entity));
            }

            if (returnObject)
            {
                if (dbType == DatabaseType.Postgres)
                {
                    return qb.InsertSql + $" RETURNING {qb.ReturnStatement}; ";
                }
                return qb.InsertReturnObjectSql;
            }

            return AppendIdentityReturn(qb.InsertSql, dbType, returnIdentity, metadata.PrimaryKeyProperty?.ColumnName);
        }

        /// <summary>
        /// Generates an INSERT statement with parameter placeholders (using : prefix).
        /// </summary>
        public static string GenerateInsertStatement<TEntity>(
            DatabaseType dbType,
            bool returnIdentity) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var qb = new FastQueryBuilder(tableName);

            // Add identity column if present
            if (metadata.PrimaryKeyProperty != null)
            {
                qb.AddIdentityColumn(metadata.PrimaryKeyProperty.PropertyName);
            }

            // Add insertable properties with positional parameters
            foreach (var prop in metadata.InsertableProperties)
            {
                qb.Add(prop.ColumnName, prop.PropertyName, $":{prop.PropertyName}");
            }

            return AppendIdentityReturn(qb.InsertSql, dbType, returnIdentity, metadata.PrimaryKeyProperty?.ColumnName);
        }

        /// <summary>
        /// Generates an UPDATE query with actual values from the entity.
        /// </summary>
        public static string GenerateUpdateQuery<TEntity>(
            TEntity entity,
            string where,
            DatabaseType dbType) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var qb = new FastQueryBuilder(tableName, $" WHERE {where}");

            foreach (var prop in metadata.UpdatableProperties)
            {
                var paramName = prop.GetParameterName(dbType == DatabaseType.Postgres);
                qb.Add(prop.ColumnName, paramName, prop.GetValue(entity));
            }

            return qb.UpdateSql;
        }

        /// <summary>
        /// Generates an UPDATE statement with parameter placeholders (using : prefix).
        /// </summary>
        public static string GenerateUpdateStatement<TEntity>(
            string where) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var qb = new FastQueryBuilder(tableName, $" WHERE {where}");

            foreach (var prop in metadata.UpdatableProperties)
            {
                qb.Add(prop.ColumnName, prop.PropertyName, $":{prop.PropertyName}");
            }

            return qb.UpdateSql;
        }

        /// <summary>
        /// Generates a SELECT query with column aliasing.
        /// </summary>
        public static string GenerateSelectQuery<TEntity>(string where) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var qb = new FastQueryBuilder(tableName, " WHERE " + where);

            foreach (var prop in metadata.SelectableProperties)
            {
                qb.Add(prop.ColumnName, prop.PropertyName, "");
            }

            return qb.SelectSql;
        }

        /// <summary>
        /// Generates a SELECT query with specific columns, where clause, and top/limit.
        /// </summary>
        public static string GenerateSelectQueryWithColumns<TEntity>(
            string[] columns,
            string where,
            int top,
            DatabaseType dbType) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();

            string columnList;
            if (columns is { Length: > 0 })
            {
                columnList = dbType == DatabaseType.SqlServer
                    ? $"[{string.Join("],[", columns.Select(i => i.Replace("[", "").Replace("]", "")))}]"
                    : string.Join(", ", columns);
            }
            else
            {
                columnList = "*";
            }

            return dbType switch
            {
                DatabaseType.SqlServer => $"SELECT TOP({top}) {columnList} FROM {tableName} WHERE {where};",
                DatabaseType.Oracle => $"SELECT {columnList} FROM {tableName} WHERE {where} FETCH FIRST {top} ROWS ONLY;",
                _ => $"SELECT {columnList} FROM {tableName} WHERE {where} LIMIT {top};" // Postgres, MySQL, SQLite
            };
        }

        /// <summary>
        /// Generates a DELETE query.
        /// </summary>
        public static string GenerateDeleteQuery<TEntity>(string where) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            return $"DELETE FROM {tableName} WHERE {where};";
        }

        /// <summary>
        /// Appends the identity return clause based on database type.
        /// </summary>
        private static string AppendIdentityReturn(string sql, DatabaseType dbType, bool returnIdentity, string? primaryKeyColumn = null)
        {
            if (!returnIdentity)
                return sql;

            var pk = primaryKeyColumn ?? "id";

            return dbType switch
            {
                DatabaseType.Postgres => sql + $" RETURNING {pk}; ",
                DatabaseType.MySql => sql + "; SELECT LAST_INSERT_ID();",
                DatabaseType.SQLite => sql + "; SELECT last_insert_rowid();",
                DatabaseType.Oracle => sql + $" RETURNING {pk} INTO :{pk}",
                _ => sql + "; SELECT SCOPE_IDENTITY();" // SqlServer default
            };
        }
    }
}
