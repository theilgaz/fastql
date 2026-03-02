using Xunit;

namespace Fastql.Tests;

public class BackwardCompatibilityTests
{
    [Fact]
    public void TableName_WithDefaultOutput_ReturnsBracketedFormat()
    {
        var builder = new FastqlBuilder<Customer>();
        var tableName = builder.TableName();

        Assert.Equal("[dbo].[Customers]", tableName);
    }

    [Fact]
    public void TableName_WithTableAndSchemaOutput_ReturnsDotFormat()
    {
        var builder = new FastqlBuilder<Product>();
        var tableName = builder.TableName();

        Assert.Equal("inventory.Products", tableName);
    }

    [Fact]
    public void TableName_WithOnlyTableOutput_ReturnsTableNameOnly()
    {
        var builder = new FastqlBuilder<Order>();
        var tableName = builder.TableName();

        Assert.Equal("Orders", tableName);
    }

    [Fact]
    public void InsertQuery_ExcludesPrimaryKey()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.InsertQuery();

        Assert.DoesNotContain("@Id", query);
        Assert.Contains("@Name", query);
        Assert.Contains("@Email", query);
    }

    [Fact]
    public void InsertQuery_ExcludesSelectOnlyProperties()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.InsertQuery();

        Assert.DoesNotContain("LastLoginAt", query);
    }

    [Fact]
    public void InsertQuery_ExcludesCustomFieldProperties()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.InsertQuery();

        Assert.DoesNotContain("FullDescription", query);
    }

    [Fact]
    public void InsertQuery_ExcludesIsNotInsertableProperties()
    {
        var builder = new FastqlBuilder<Product>();
        var query = builder.InsertQuery();

        Assert.DoesNotContain("StockCount", query);
    }

    [Fact]
    public void InsertQuery_WithReturnIdentity_SqlServer_ReturnsWithScopeIdentity()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("SELECT SCOPE_IDENTITY()", query);
    }

    [Fact]
    public void InsertQuery_WithReturnIdentity_Postgres_ReturnsWithReturningId()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.Postgres);
        var query = builder.InsertQuery(returnIdentity: true);

        Assert.Contains("RETURNING Id", query);
    }

    [Fact]
    public void InsertQuery_UsesFieldAttributeColumnNames()
    {
        var builder = new FastqlBuilder<Product>();
        var query = builder.InsertQuery();

        Assert.Contains("product_name", query);
        Assert.Contains("unit_price", query);
    }

    [Fact]
    public void UpdateQuery_ExcludesPrimaryKey()
    {
        var builder = new FastqlBuilder<Customer>();
        var customer = new Customer { Id = 1, Name = "Test", Email = "test@test.com" };
        var query = builder.UpdateQuery(customer, "Id = @Id");

        Assert.DoesNotContain("Id = @Id,", query);
        Assert.Contains("Name = @Name", query);
    }

    [Fact]
    public void UpdateQuery_ExcludesIsNotUpdatableProperties()
    {
        var builder = new FastqlBuilder<Customer>();
        var customer = new Customer { Id = 1, Name = "Test", Email = "test@test.com" };
        var query = builder.UpdateQuery(customer, "Id = @Id");

        Assert.DoesNotContain("CreatedAt", query);
    }

    [Fact]
    public void UpdateQuery_ExcludesSelectOnlyProperties()
    {
        var builder = new FastqlBuilder<Customer>();
        var customer = new Customer { Id = 1, Name = "Test", Email = "test@test.com" };
        var query = builder.UpdateQuery(customer, "Id = @Id");

        Assert.DoesNotContain("LastLoginAt", query);
    }

    [Fact]
    public void DeleteQuery_GeneratesCorrectFormat()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.DeleteQuery("Id = @Id");

        Assert.Equal("DELETE FROM [dbo].[Customers] WHERE Id = @Id;", query);
    }

    [Fact]
    public void SelectQuery_ExcludesCustomFieldProperties()
    {
        var builder = new FastqlBuilder<Customer>();
        var query = builder.SelectQuery("Id = @Id");

        Assert.DoesNotContain("FullDescription", query);
    }

    [Fact]
    public void InsertStatement_UsesAtParameterFormat()
    {
        // InsertStatement uses the same @ParameterName format as InsertQuery
        // The original implementation passes :PropertyName as value but FastQueryBuilder.InsertSql
        // ignores the value and uses @Name format for the VALUES clause
        var builder = new FastqlBuilder<Customer>();
        var query = builder.InsertStatement();

        Assert.Contains("@Name", query);
        Assert.Contains("@Email", query);
    }

    [Fact]
    public void FastqlHelper_InsertQuery_WorksIdentically()
    {
        FastqlHelper<Customer>.SetDatabaseType(DatabaseType.SqlServer);
        var query = FastqlHelper<Customer>.InsertQuery();

        Assert.Contains("INSERT INTO [dbo].[Customers]", query);
        Assert.Contains("@Name", query);
        Assert.DoesNotContain("@Id", query);
    }

    [Fact]
    public void FastqlHelper_DeleteQuery_WorksIdentically()
    {
        var query = FastqlHelper<Customer>.DeleteQuery("Id = @Id");

        Assert.Equal("DELETE FROM [dbo].[Customers] WHERE Id = @Id;", query);
    }
}
