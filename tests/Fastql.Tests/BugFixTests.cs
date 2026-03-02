using Xunit;

namespace Fastql.Tests;

public class BugFixTests
{
    [Fact]
    public void InsertReturnObjectQuery_SqlServer_OutputInsertedHasNoSpaces()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.InsertReturnObjectQuery();

        Assert.Contains("OUTPUT inserted.*", query);
        Assert.DoesNotContain("OUTPUT inserted . *", query);
    }

    [Fact]
    public void InsertQuery_Postgres_ReturnIdentity_UsesActualPkColumnName()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("RETURNING Id", query);
        Assert.DoesNotContain("RETURNING ID", query);
    }

    [Fact]
    public void InsertQuery_Postgres_ReturnIdentity_UsesFieldAttributeColumnName()
    {
        var builder = new FastqlBuilder<Product>(DatabaseType.Postgres);
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("RETURNING ProductId", query);
    }

    [Fact]
    public void SelectQuery_Postgres_UsesLimit()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.SelectQuery(new[] { "Name", "Email" }, "Id = @Id", 10);

        Assert.Contains("LIMIT 10", query);
        Assert.DoesNotContain("TOP", query);
    }

    [Fact]
    public void SelectQuery_MySql_UsesLimit()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.MySql);
        var query = builder.SelectQuery(new[] { "Name" }, "Id = @Id", 50);

        Assert.Contains("LIMIT 50", query);
        Assert.DoesNotContain("TOP", query);
    }

    [Fact]
    public void SelectQuery_SQLite_UsesLimit()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SQLite);
        var query = builder.SelectQuery(new[] { "Name" }, "Id = @Id", 25);

        Assert.Contains("LIMIT 25", query);
    }

    [Fact]
    public void SelectQuery_Oracle_UsesFetchFirst()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Oracle);
        var query = builder.SelectQuery(new[] { "Name" }, "Id = @Id", 15);

        Assert.Contains("FETCH FIRST 15 ROWS ONLY", query);
        Assert.DoesNotContain("TOP", query);
        Assert.DoesNotContain("LIMIT", query);
    }

    [Fact]
    public void SelectQuery_SqlServer_UsesTop()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.SelectQuery(new[] { "Name", "Email" }, "Id = @Id", 100);

        Assert.Contains("TOP(100)", query);
    }

    [Fact]
    public void SelectQuery_SqlServer_BracketsColumns()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.SelectQuery(new[] { "Name", "Email" }, "Id = @Id", 10);

        Assert.Contains("[Name]", query);
        Assert.Contains("[Email]", query);
    }

    [Fact]
    public void SelectQuery_Postgres_DoesNotBracketColumns()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.SelectQuery(new[] { "Name", "Email" }, "Id = @Id", 10);

        Assert.DoesNotContain("[Name]", query);
        Assert.DoesNotContain("[Email]", query);
        Assert.Contains("Name", query);
        Assert.Contains("Email", query);
    }

    [Fact]
    public void SelectQuery_EmptyColumns_UsesWildcard()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.SelectQuery(Array.Empty<string>(), "Id = @Id", 10);

        Assert.Contains("*", query);
    }

    [Fact]
    public void FastqlHelper_SelectQuery_UsesQueryGenerator()
    {
        FastqlHelper<Customer>.DatabaseType = DatabaseType.SqlServer;
        var query = FastqlHelper<Customer>.SelectQuery("Id = @Id");

        // Should use QueryGenerator which includes column aliases, not just SELECT *
        Assert.Contains("SELECT", query);
        Assert.Contains("FROM", query);
        Assert.Contains("[dbo].[Customers]", query);
        Assert.Contains("WHERE Id = @Id", query);
    }

    [Fact]
    public void FastqlHelper_SelectQueryWithColumns_UsesQueryGenerator()
    {
        FastqlHelper<Customer>.DatabaseType = DatabaseType.Postgres;
        var query = FastqlHelper<Customer>.SelectQuery(new[] { "Name" }, "Id = @Id", 5);

        Assert.Contains("LIMIT 5", query);
        Assert.DoesNotContain("TOP", query);
    }
}
