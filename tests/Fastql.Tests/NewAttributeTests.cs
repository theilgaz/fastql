using Fastql.Caching;
using Xunit;

namespace Fastql.Tests;

public class NewAttributeTests
{
    [Fact]
    public void ColumnAttribute_MapsToCorrectColumnName()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Setting>();
        var keyProperty = metadata.Properties.First(p => p.PropertyName == "Key");
        var valueProperty = metadata.Properties.First(p => p.PropertyName == "Value");

        Assert.Equal("setting_key", keyProperty.ColumnName);
        Assert.Equal("setting_value", valueProperty.ColumnName);
    }

    [Fact]
    public void ColumnAttribute_WorksInInsertQuery()
    {
        var builder = new FastqlBuilder<Setting>();
        var query = builder.InsertQuery();

        Assert.Contains("setting_key", query);
        Assert.Contains("setting_value", query);
    }

    [Fact]
    public void IgnoreAttribute_ExcludesFromAllQueries()
    {
        var builder = new FastqlBuilder<Order>();

        var insertQuery = builder.InsertQuery();
        var order = new Order { OrderId = 1, CustomerId = 10, TotalAmount = 100m };
        var updateQuery = builder.UpdateQuery(order, "OrderId = @OrderId");
        var selectQuery = builder.SelectQuery("OrderId = @OrderId");

        // Check that the navigation property "Customer" (not "CustomerId") is excluded
        // We check for @Customer which would indicate the property is included
        Assert.DoesNotContain("@Customer,", insertQuery);
        Assert.DoesNotContain("@Customer)", insertQuery);
        Assert.DoesNotContain("Customer =", updateQuery);
        Assert.DoesNotContain("Customer as", selectQuery);
    }

    [Fact]
    public void IgnoreAttribute_PropertyMetadataIsIgnored()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Order>();
        var customerProperty = metadata.Properties.FirstOrDefault(p => p.PropertyName == "Customer");

        Assert.NotNull(customerProperty);
        Assert.True(customerProperty.IsIgnored);
        Assert.False(customerProperty.IsInsertable);
        Assert.False(customerProperty.IsUpdatable);
        Assert.False(customerProperty.IsSelectable);
    }

    [Fact]
    public void RequiredAttribute_IsCachedInPropertyMetadata()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Setting>();
        var descriptionProperty = metadata.Properties.First(p => p.PropertyName == "Description");

        Assert.True(descriptionProperty.IsRequired);
    }

    [Fact]
    public void MaxLengthAttribute_IsCachedInPropertyMetadata()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Setting>();
        var descriptionProperty = metadata.Properties.First(p => p.PropertyName == "Description");

        Assert.Equal(100, descriptionProperty.MaxLength);
    }

    [Fact]
    public void DefaultValueAttribute_IsCachedInPropertyMetadata()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Setting>();
        var isActiveProperty = metadata.Properties.First(p => p.PropertyName == "IsActive");

        Assert.Equal(true, isActiveProperty.DefaultValue);
    }

    [Fact]
    public void MaxLengthAttribute_ThrowsForNegativeLength()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new MaxLengthAttribute(-1));
    }
}
