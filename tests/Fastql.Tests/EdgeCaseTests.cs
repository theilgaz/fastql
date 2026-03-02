using Fastql.Caching;
using Fastql.Exceptions;
using Xunit;

namespace Fastql.Tests;

public class EdgeCaseTests
{
    [Fact]
    public void ImplicitTableEntity_UsesClassNameAsTableName()
    {
        var metadata = TypeMetadataCache.GetOrCreate<ImplicitTableEntity>();

        Assert.Equal("ImplicitTableEntity", metadata.TableName);
        Assert.Equal("dbo", metadata.Schema);
        Assert.Equal(OutputName.Default, metadata.OutputFormat);
    }

    [Fact]
    public void ImplicitTableEntity_GeneratesCorrectQueries()
    {
        var builder = new FastqlBuilder<ImplicitTableEntity>();
        var query = builder.InsertQuery();

        Assert.Contains("[dbo].[ImplicitTableEntity]", query);
        Assert.Contains("@Name", query);
    }

    [Fact]
    public void NoPrimaryKeyEntity_HasNullPrimaryKey()
    {
        var metadata = TypeMetadataCache.GetOrCreate<NoPrimaryKeyEntity>();

        Assert.Null(metadata.PrimaryKeyProperty);
    }

    [Fact]
    public void NoPrimaryKeyEntity_InsertQueryWorks()
    {
        var builder = new FastqlBuilder<NoPrimaryKeyEntity>();
        var query = builder.InsertQuery();

        Assert.Contains("INSERT INTO", query);
        Assert.Contains("@Name", query);
        Assert.Contains("@Value", query);
    }

    [Fact]
    public void NoPrimaryKeyEntity_InsertQueryWithReturnIdentity_NoIdentityClause()
    {
        var builder = new FastqlBuilder<NoPrimaryKeyEntity>(DatabaseType.Postgres);
        var query = builder.InsertQuery(returnIdentity: true);

        // With no PK, it should use the default "id" fallback
        Assert.Contains("RETURNING id", query);
    }

    [Fact]
    public void AllExcludedEntity_InsertQuery_ThrowsMissingParametersException()
    {
        var builder = new FastqlBuilder<AllExcludedEntity>();

        Assert.Throws<MissingParametersException>(() => builder.InsertQuery());
    }

    [Fact]
    public void AllExcludedEntity_HasNonSelectableProperties()
    {
        var metadata = TypeMetadataCache.GetOrCreate<AllExcludedEntity>();

        Assert.Empty(metadata.InsertableProperties);
        Assert.Empty(metadata.UpdatableProperties);
        // PK and Ignored are selectable differently:
        // PK (Id) is selectable, Ignored is not, CustomField is not
        Assert.Single(metadata.SelectableProperties); // Only Id
    }

    [Fact]
    public void QueryBuilderObject_Equals_ByKey()
    {
        var obj1 = new QueryBuilderObject("Name", "Name", "value1");
        var obj2 = new QueryBuilderObject("Name", "DifferentName", "value2");

        Assert.True(obj1.Equals(obj2));
    }

    [Fact]
    public void QueryBuilderObject_NotEquals_DifferentKeys()
    {
        var obj1 = new QueryBuilderObject("Name", "Name", "value1");
        var obj2 = new QueryBuilderObject("Email", "Email", "value2");

        Assert.False(obj1.Equals(obj2));
    }

    [Fact]
    public void QueryBuilderObject_GetHashCode_ConsistentWithEquals()
    {
        var obj1 = new QueryBuilderObject("Name", "Name1", "v1");
        var obj2 = new QueryBuilderObject("Name", "Name2", "v2");

        Assert.Equal(obj1.GetHashCode(), obj2.GetHashCode());
    }

    [Fact]
    public void FastqlHelper_TableName_IsPublic()
    {
        var tableName = FastqlHelper<Customer>.TableName();

        Assert.Equal("[dbo].[Customers]", tableName);
    }

    [Fact]
    public void FastqlHelper_ObsoleteMethods_StillWork()
    {
#pragma warning disable CS0618
        FastqlHelper<Customer>.SetDatabaseType(DatabaseType.SqlServer);
        var dbType = FastqlHelper<Customer>.GetDatabaseType();
#pragma warning restore CS0618

        Assert.Equal(DatabaseType.SqlServer, dbType);
    }

    [Fact]
    public void SelectQuery_WithFieldAttribute_UsesColumnNameAlias()
    {
        var builder = new FastqlBuilder<Product>();
        var query = builder.SelectQuery("ProductId = @ProductId");

        Assert.Contains("product_name", query);
        Assert.Contains("unit_price", query);
    }

    [Fact]
    public void DeleteQuery_WithOnlyTableOutput_UsesTableNameOnly()
    {
        var builder = new FastqlBuilder<Order>();
        var query = builder.DeleteQuery("OrderId = @OrderId");

        Assert.Equal("DELETE FROM Orders WHERE OrderId = @OrderId;", query);
    }
}
