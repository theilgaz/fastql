using System;
using System.Linq;
using System.Text;
using Fastql.Caching;
using Fastql.Exceptions;

namespace Fastql.Core
{
    internal static class UpsertQueryGenerator
    {
        public static string UpsertQuery<TEntity>(DatabaseType dbType) where TEntity : class
        {
            var metadata = TypeMetadataCache.GetOrCreate<TEntity>();
            var tableName = metadata.GetFormattedTableName();
            var pk = metadata.PrimaryKeyProperty;
            var insertableProps = metadata.InsertableProperties;
            var updatableProps = metadata.UpdatableProperties;

            if (pk == null)
                throw new MissingParametersException("Primary key is required for upsert.");

            if (insertableProps.Count == 0)
                throw new MissingParametersException();

            return dbType switch
            {
                DatabaseType.SqlServer => GenerateSqlServerMerge(tableName, pk, insertableProps, updatableProps),
                DatabaseType.Postgres => GeneratePostgresUpsert(tableName, pk, insertableProps, updatableProps, useTypeCast: true),
                DatabaseType.MySql => GenerateMySqlUpsert(tableName, pk, insertableProps, updatableProps),
                DatabaseType.SQLite => GenerateSQLiteUpsert(tableName, insertableProps, pk),
                DatabaseType.Oracle => GenerateOracleMerge(tableName, pk, insertableProps, updatableProps),
                _ => throw new FastqlException($"Unsupported database type for upsert: {dbType}")
            };
        }

        private static string GenerateSqlServerMerge(
            string tableName,
            PropertyMetadata pk,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> insertableProps,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> updatableProps)
        {
            var sb = new StringBuilder();
            var insertColumns = string.Join(", ", insertableProps.Select(p => p.ColumnName));
            var insertValues = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));

            sb.Append($"MERGE INTO {tableName} AS target ");
            sb.Append($"USING (SELECT @{pk.PropertyName} AS {pk.ColumnName}) AS source ");
            sb.Append($"ON target.{pk.ColumnName} = source.{pk.ColumnName} ");

            if (updatableProps.Count > 0)
            {
                var updateClauses = string.Join(", ", updatableProps.Select(p => $"target.{p.ColumnName} = @{p.PropertyName}"));
                sb.Append($"WHEN MATCHED THEN UPDATE SET {updateClauses} ");
            }

            sb.Append($"WHEN NOT MATCHED THEN INSERT ({insertColumns}) VALUES({insertValues});");

            return sb.ToString();
        }

        private static string GeneratePostgresUpsert(
            string tableName,
            PropertyMetadata pk,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> insertableProps,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> updatableProps,
            bool useTypeCast)
        {
            var insertColumns = string.Join(", ", insertableProps.Select(p => p.ColumnName));
            var insertValues = string.Join(", ", insertableProps.Select(p => $"@{p.GetParameterName(useTypeCast)}"));

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {tableName}({insertColumns}) VALUES({insertValues}) ");
            sb.Append($"ON CONFLICT ({pk.ColumnName}) DO UPDATE SET ");

            var updateClauses = string.Join(", ", updatableProps.Select(p => $"{p.ColumnName} = @{p.GetParameterName(useTypeCast)}"));
            sb.Append(updateClauses);
            sb.Append(';');

            return sb.ToString();
        }

        private static string GenerateMySqlUpsert(
            string tableName,
            PropertyMetadata pk,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> insertableProps,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> updatableProps)
        {
            var insertColumns = string.Join(", ", insertableProps.Select(p => p.ColumnName));
            var insertValues = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));

            var sb = new StringBuilder();
            sb.Append($"INSERT INTO {tableName}({insertColumns}) VALUES({insertValues}) ");
            sb.Append("ON DUPLICATE KEY UPDATE ");

            var updateClauses = string.Join(", ", updatableProps.Select(p => $"{p.ColumnName} = @{p.PropertyName}"));
            sb.Append(updateClauses);
            sb.Append(';');

            return sb.ToString();
        }

        private static string GenerateSQLiteUpsert(
            string tableName,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> insertableProps,
            PropertyMetadata pk)
        {
            var insertColumns = string.Join(", ", insertableProps.Select(p => p.ColumnName));
            var insertValues = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));

            return $"INSERT OR REPLACE INTO {tableName}({insertColumns}) VALUES({insertValues});";
        }

        private static string GenerateOracleMerge(
            string tableName,
            PropertyMetadata pk,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> insertableProps,
            System.Collections.Generic.IReadOnlyList<PropertyMetadata> updatableProps)
        {
            var sb = new StringBuilder();
            var insertColumns = string.Join(", ", insertableProps.Select(p => p.ColumnName));
            var insertValues = string.Join(", ", insertableProps.Select(p => $"@{p.PropertyName}"));

            sb.Append($"MERGE INTO {tableName} target ");
            sb.Append($"USING (SELECT @{pk.PropertyName} AS {pk.ColumnName} FROM DUAL) source ");
            sb.Append($"ON (target.{pk.ColumnName} = source.{pk.ColumnName}) ");

            if (updatableProps.Count > 0)
            {
                var updateClauses = string.Join(", ", updatableProps.Select(p => $"target.{p.ColumnName} = @{p.PropertyName}"));
                sb.Append($"WHEN MATCHED THEN UPDATE SET {updateClauses} ");
            }

            sb.Append($"WHEN NOT MATCHED THEN INSERT ({insertColumns}) VALUES({insertValues})");
            return sb.ToString();
        }
    }
}
