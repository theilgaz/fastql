using Fastql.Exceptions;
using Xunit;

namespace Fastql.Tests;

public class BulkOperationTests
{
    [Fact]
    public void BulkInsertQuery_SqlServer_GeneratesMultiRowInsert()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var entities = new List<Customer>
        {
            new() { Name = "Alice", Email = "alice@test.com" },
            new() { Name = "Bob", Email = "bob@test.com" }
        };

        var query = builder.BulkInsertQuery(entities);

        Assert.Contains("INSERT INTO [dbo].[Customers]", query);
        Assert.Contains("@Name_0", query);
        Assert.Contains("@Email_0", query);
        Assert.Contains("@Name_1", query);
        Assert.Contains("@Email_1", query);
    }

    [Fact]
    public void BulkInsertQuery_Postgres_GeneratesWithTypeCast()
    {
        var builder = new FastqlBuilder<PostgresLog>(DatabaseType.Postgres);
        var entities = new List<PostgresLog>
        {
            new() { Message = "log1", Metadata = "{}" },
            new() { Message = "log2", Metadata = "{}" }
        };

        var query = builder.BulkInsertQuery(entities);

        Assert.Contains("@Metadata::jsonb_0", query);
        Assert.Contains("@CreatedAt::timestamp_0", query);
        Assert.Contains("@Metadata::jsonb_1", query);
    }

    [Fact]
    public void BulkInsertQuery_Oracle_GeneratesInsertAll()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Oracle);
        var entities = new List<Customer>
        {
            new() { Name = "Alice" },
            new() { Name = "Bob" }
        };

        var query = builder.BulkInsertQuery(entities);

        Assert.StartsWith("INSERT ALL", query);
        Assert.Contains("INTO [dbo].[Customers]", query);
        Assert.Contains("SELECT 1 FROM DUAL", query);
        Assert.Contains("@Name_0", query);
        Assert.Contains("@Name_1", query);
    }

    [Fact]
    public void BulkInsertQuery_SingleEntity_Works()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var entities = new List<Customer> { new() { Name = "Alice" } };

        var query = builder.BulkInsertQuery(entities);

        Assert.Contains("@Name_0", query);
        Assert.DoesNotContain("@Name_1", query);
    }

    [Fact]
    public void BulkInsertQuery_EmptyList_ThrowsMissingParametersException()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);

        Assert.Throws<MissingParametersException>(() => builder.BulkInsertQuery(new List<Customer>()));
    }

    [Fact]
    public void BulkUpdateQuery_SqlServer_GeneratesConcatenatedUpdates()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var entities = new List<Customer>
        {
            new() { Id = 1, Name = "Alice", Email = "alice@test.com" },
            new() { Id = 2, Name = "Bob", Email = "bob@test.com" }
        };

        var query = builder.BulkUpdateQuery(entities);

        Assert.Contains("UPDATE [dbo].[Customers]", query);
        Assert.Contains("Name = @Name_0", query);
        Assert.Contains("Email = @Email_0", query);
        Assert.Contains("WHERE Id = @Id_0", query);
        Assert.Contains("Name = @Name_1", query);
        Assert.Contains("WHERE Id = @Id_1", query);
    }

    [Fact]
    public void BulkUpdateQuery_ExcludesNotUpdatableProperties()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var entities = new List<Customer> { new() { Id = 1, Name = "Test" } };

        var query = builder.BulkUpdateQuery(entities);

        Assert.DoesNotContain("CreatedAt", query);
        Assert.DoesNotContain("LastLoginAt", query);
    }

    [Fact]
    public void BulkUpdateQuery_EmptyList_ThrowsMissingParametersException()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);

        Assert.Throws<MissingParametersException>(() => builder.BulkUpdateQuery(new List<Customer>()));
    }

    [Fact]
    public void BulkUpdateQuery_NoPrimaryKey_ThrowsMissingParametersException()
    {
        var builder = new FastqlBuilder<NoPrimaryKeyEntity>(DatabaseType.SqlServer);
        var entities = new List<NoPrimaryKeyEntity> { new() { Name = "Test" } };

        Assert.Throws<MissingParametersException>(() => builder.BulkUpdateQuery(entities));
    }

    [Fact]
    public void FastqlHelper_BulkInsertQuery_Works()
    {
        FastqlHelper<Customer>.DatabaseType = DatabaseType.SqlServer;
        var entities = new List<Customer> { new() { Name = "Test" } };

        var query = FastqlHelper<Customer>.BulkInsertQuery(entities);

        Assert.Contains("INSERT INTO", query);
        Assert.Contains("@Name_0", query);
    }

    [Fact]
    public void FastqlHelper_BulkUpdateQuery_Works()
    {
        FastqlHelper<Customer>.DatabaseType = DatabaseType.SqlServer;
        var entities = new List<Customer> { new() { Id = 1, Name = "Test" } };

        var query = FastqlHelper<Customer>.BulkUpdateQuery(entities);

        Assert.Contains("UPDATE", query);
        Assert.Contains("@Name_0", query);
    }
}
