using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Fastql.Caching;
using Fastql.Exceptions;

namespace Fastql.Core
{
    internal static class BulkQueryGenerator
    {
        public static string BulkInsertQuery<TEntity>(IReadOnlyList<TEntity> entities, DatabaseType dbType) where TEntity : class
        {
            if (entities == null || entities.Count == 0)
                throw new MissingParametersException("No entities provided for bulk insert.");

            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var insertableProps = metadata.InsertableProperties;

            if (insertableProps.Count == 0)
                throw new MissingParametersException();

            var columns = string.Join(", ", insertableProps.Select(p => p.ColumnName));

            if (dbType == DatabaseType.Oracle)
                return GenerateOracleBulkInsert(entities, tableName, insertableProps, columns);

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {tableName}({columns}) VALUES ");

            var rows = new List<string>();
            for (var i = 0; i < entities.Count; i++)
            {
                var paramNames = insertableProps.Select(p =>
                {
                    var paramName = p.GetParameterName(dbType == DatabaseType.Postgres);
                    return $"@{paramName}_{i}";
                });
                rows.Add($"({string.Join(", ", paramNames)})");
            }

            sb.Append(string.Join(", ", rows));
            sb.Append(';');
            return sb.ToString();
        }

        public static string BulkUpdateQuery<TEntity>(IReadOnlyList<TEntity> entities, DatabaseType dbType) where TEntity : class
        {
            if (entities == null || entities.Count == 0)
                throw new MissingParametersException("No entities provided for bulk update.");

            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var updatableProps = metadata.UpdatableProperties;
            var pk = metadata.PrimaryKeyProperty;

            if (updatableProps.Count == 0)
                throw new MissingParametersException();

            if (pk == null)
                throw new MissingParametersException("Primary key is required for bulk update.");

            var sb = new StringBuilder();

            for (var i = 0; i < entities.Count; i++)
            {
                var setClauses = updatableProps.Select(p =>
                {
                    var paramName = p.GetParameterName(dbType == DatabaseType.Postgres);
                    return $"{p.ColumnName} = @{paramName}_{i}";
                });

                sb.Append($"UPDATE {tableName} SET {string.Join(", ", setClauses)} WHERE {pk.ColumnName} = @{pk.PropertyName}_{i};");

                if (i < entities.Count - 1)
                    sb.Append(' ');
            }

            return sb.ToString();
        }

        private static string GenerateOracleBulkInsert<TEntity>(
            IReadOnlyList<TEntity> entities,
            string tableName,
            IReadOnlyList<PropertyMetadata> insertableProps,
            string columns) where TEntity : class
        {
            var sb = new StringBuilder();
            sb.Append("INSERT ALL ");

            for (var i = 0; i < entities.Count; i++)
            {
                var paramNames = insertableProps.Select(p => $"@{p.PropertyName}_{i}");
                sb.Append($"INTO {tableName}({columns}) VALUES({string.Join(", ", paramNames)}) ");
            }

            sb.Append("SELECT 1 FROM DUAL");
            return sb.ToString();
        }
    }
}
