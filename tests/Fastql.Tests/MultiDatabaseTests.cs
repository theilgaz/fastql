using Xunit;

namespace Fastql.Tests;

public class MultiDatabaseTests
{
    [Fact]
    public void DatabaseType_PreservesExistingOrdinals()
    {
        Assert.Equal(0, (int)DatabaseType.SqlServer);
        Assert.Equal(1, (int)DatabaseType.Postgres);
    }

    [Fact]
    public void DatabaseType_HasNewDatabaseTypes()
    {
        Assert.Equal(2, (int)DatabaseType.MySql);
        Assert.Equal(3, (int)DatabaseType.SQLite);
        Assert.Equal(4, (int)DatabaseType.Oracle);
    }

    [Fact]
    public void InsertQuery_MySql_ReturnsLastInsertId()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.MySql);
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("SELECT LAST_INSERT_ID()", query);
    }

    [Fact]
    public void InsertQuery_SQLite_ReturnsLastInsertRowId()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SQLite);
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("SELECT last_insert_rowid()", query);
    }

    [Fact]
    public void InsertQuery_Oracle_NoReturnClause()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Oracle);
        var query = builder.InsertQuery(returnIdentity: true);

        // Oracle uses RETURNING INTO with bind variables, which is different syntax
        Assert.DoesNotContain("SELECT", query);
        Assert.DoesNotContain("RETURNING", query);
    }

    [Fact]
    public void PostgresFieldTypes_AreHandledCorrectly()
    {
        var builder = new FastqlBuilder<PostgresLog>(DatabaseType.Postgres);
        var query = builder.InsertQuery();

        Assert.Contains("@Metadata::jsonb", query);
        Assert.Contains("@CreatedAt::timestamp", query);
    }

    [Fact]
    public void PostgresFieldTypes_NotAppliedForSqlServer()
    {
        var builder = new FastqlBuilder<PostgresLog>(DatabaseType.SqlServer);
        var query = builder.InsertQuery();

        Assert.DoesNotContain("::jsonb", query);
        Assert.DoesNotContain("::timestamp", query);
    }

    [Fact]
    public void InsertReturnObjectQuery_Postgres_UsesReturningClause()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.InsertReturnObjectQuery();

        Assert.Contains("RETURNING", query);
    }

    [Fact]
    public void InsertReturnObjectQuery_SqlServer_UsesOutputInserted()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        var query = builder.InsertReturnObjectQuery();

        Assert.Contains("OUTPUT inserted", query);
    }
}
