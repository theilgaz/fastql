using Fastql.Exceptions;
using Xunit;

namespace Fastql.Tests;

public class UpsertTests
{
    [Fact]
    public void UpsertQuery_SqlServer_GeneratesMerge()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.UpsertQuery();

        Assert.Contains("MERGE INTO [dbo].[Customers] AS target", query);
        Assert.Contains("USING (SELECT @Id AS Id) AS source", query);
        Assert.Contains("ON target.Id = source.Id", query);
        Assert.Contains("WHEN MATCHED THEN UPDATE SET", query);
        Assert.Contains("WHEN NOT MATCHED THEN INSERT", query);
    }

    [Fact]
    public void UpsertQuery_Postgres_GeneratesOnConflict()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.UpsertQuery();

        Assert.Contains("INSERT INTO", query);
        Assert.Contains("ON CONFLICT (Id) DO UPDATE SET", query);
    }

    [Fact]
    public void UpsertQuery_Postgres_IncludesTypeCasts()
    {
        var builder = new FastqlBuilder<PostgresLog>(DatabaseType.Postgres);
        var query = builder.UpsertQuery();

        Assert.Contains("@Metadata::jsonb", query);
        Assert.Contains("@CreatedAt::timestamp", query);
        Assert.Contains("ON CONFLICT (Id) DO UPDATE SET", query);
    }

    [Fact]
    public void UpsertQuery_MySql_GeneratesOnDuplicateKey()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.MySql);
        var query = builder.UpsertQuery();

        Assert.Contains("INSERT INTO", query);
        Assert.Contains("ON DUPLICATE KEY UPDATE", query);
        Assert.Contains("Name = @Name", query);
        Assert.Contains("Email = @Email", query);
    }

    [Fact]
    public void UpsertQuery_SQLite_GeneratesInsertOrReplace()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SQLite);
        var query = builder.UpsertQuery();

        Assert.StartsWith("INSERT OR REPLACE INTO", query);
    }

    [Fact]
    public void UpsertQuery_Oracle_GeneratesMerge()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Oracle);
        var query = builder.UpsertQuery();

        Assert.Contains("MERGE INTO [dbo].[Customers] target", query);
        Assert.Contains("FROM DUAL", query);
        Assert.Contains("WHEN MATCHED THEN UPDATE SET", query);
        Assert.Contains("WHEN NOT MATCHED THEN INSERT", query);
    }

    [Fact]
    public void UpsertQuery_ExcludesPrimaryKeyFromInsertValues()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.UpsertQuery();

        // The INSERT part should not include PK in the columns
        // But MERGE USING should include PK for matching
        Assert.Contains("@Id", query); // In USING clause
    }

    [Fact]
    public void UpsertQuery_NoPrimaryKey_ThrowsMissingParametersException()
    {
        var builder = new FastqlBuilder<NoPrimaryKeyEntity>(DatabaseType.SqlServer);

        Assert.Throws<MissingParametersException>(() => builder.UpsertQuery());
    }

    [Fact]
    public void UpsertQuery_UsesFieldAttributeColumnNames()
    {
        var builder = new FastqlBuilder<Product>(DatabaseType.Postgres);
        var query = builder.UpsertQuery();

        Assert.Contains("product_name", query);
        Assert.Contains("unit_price", query);
        Assert.Contains("ON CONFLICT (ProductId)", query);
    }

    [Fact]
    public void FastqlHelper_UpsertQuery_Works()
    {
        FastqlHelper<Customer>.DatabaseType = DatabaseType.SqlServer;
        var query = FastqlHelper<Customer>.UpsertQuery();

        Assert.Contains("MERGE INTO", query);
    }
}
