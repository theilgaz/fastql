using Fastql.Caching;
using Xunit;

namespace Fastql.Tests;

public class CachingTests
{
    [Fact]
    public void TypeMetadataCache_GetOrCreate_ReturnsSameInstance()
    {
        TypeMetadataCache.Clear();

        var metadata1 = TypeMetadataCache.GetOrCreate<Customer>();
        var metadata2 = TypeMetadataCache.GetOrCreate<Customer>();

        Assert.Same(metadata1, metadata2);
    }

    [Fact]
    public void TypeMetadataCache_Clear_RemovesAllCachedTypes()
    {
        TypeMetadataCache.GetOrCreate<Customer>();
        TypeMetadataCache.GetOrCreate<Product>();

        Assert.True(TypeMetadataCache.Count >= 2);

        TypeMetadataCache.Clear();

        Assert.Equal(0, TypeMetadataCache.Count);
    }

    [Fact]
    public void TypeMetadataCache_Remove_RemovesSpecificType()
    {
        // Ensure both types are cached
        TypeMetadataCache.GetOrCreate<Customer>();
        TypeMetadataCache.GetOrCreate<Product>();

        var countBefore = TypeMetadataCache.Count;
        var removed = TypeMetadataCache.Remove<Customer>();

        Assert.True(removed);
        Assert.Equal(countBefore - 1, TypeMetadataCache.Count);
    }

    [Fact]
    public void EntityMetadata_CachesTableInfo()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        Assert.Equal("Customers", metadata.TableName);
        Assert.Equal("dbo", metadata.Schema);
        Assert.Equal(OutputName.Default, metadata.OutputFormat);
    }

    [Fact]
    public void EntityMetadata_CachesPrimaryKeyProperty()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        Assert.NotNull(metadata.PrimaryKeyProperty);
        Assert.Equal("Id", metadata.PrimaryKeyProperty.PropertyName);
    }

    [Fact]
    public void EntityMetadata_PreComputesInsertableProperties()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        var insertableNames = metadata.InsertableProperties.Select(p => p.PropertyName).ToList();

        Assert.Contains("Name", insertableNames);
        Assert.Contains("Email", insertableNames);
        Assert.Contains("CreatedAt", insertableNames);
        Assert.DoesNotContain("Id", insertableNames);
        Assert.DoesNotContain("LastLoginAt", insertableNames);
        Assert.DoesNotContain("FullDescription", insertableNames);
    }

    [Fact]
    public void EntityMetadata_PreComputesUpdatableProperties()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        var updatableNames = metadata.UpdatableProperties.Select(p => p.PropertyName).ToList();

        Assert.Contains("Name", updatableNames);
        Assert.Contains("Email", updatableNames);
        Assert.DoesNotContain("Id", updatableNames);
        Assert.DoesNotContain("CreatedAt", updatableNames);
        Assert.DoesNotContain("LastLoginAt", updatableNames);
    }

    [Fact]
    public void EntityMetadata_PreComputesSelectableProperties()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        var selectableNames = metadata.SelectableProperties.Select(p => p.PropertyName).ToList();

        Assert.Contains("Id", selectableNames);
        Assert.Contains("Name", selectableNames);
        Assert.Contains("Email", selectableNames);
        Assert.Contains("CreatedAt", selectableNames);
        Assert.Contains("LastLoginAt", selectableNames);
        Assert.DoesNotContain("FullDescription", selectableNames);
    }

    [Fact]
    public void PropertyMetadata_CachesFieldAttributeInfo()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Product>();
        var nameProperty = metadata.Properties.First(p => p.PropertyName == "Name");

        Assert.Equal("product_name", nameProperty.ColumnName);
    }

    [Fact]
    public void PropertyMetadata_GetParameterName_WithTypeCast()
    {
        var metadata = TypeMetadataCache.GetOrCreate<PostgresLog>();
        var metadataProperty = metadata.Properties.First(p => p.PropertyName == "Metadata");

        Assert.Equal("Metadata::jsonb", metadataProperty.GetParameterName(includeTypeCast: true));
        Assert.Equal("Metadata", metadataProperty.GetParameterName(includeTypeCast: false));
    }

    [Fact]
    public void PropertyMetadata_CachesAllAttributeChecks()
    {
        var metadata = TypeMetadataCache.GetOrCreate<Customer>();

        var idProp = metadata.Properties.First(p => p.PropertyName == "Id");
        Assert.True(idProp.IsPrimaryKey);
        Assert.False(idProp.IsInsertable);
        Assert.False(idProp.IsUpdatable);

        var createdAtProp = metadata.Properties.First(p => p.PropertyName == "CreatedAt");
        Assert.False(createdAtProp.IsPrimaryKey);
        Assert.True(createdAtProp.IsInsertable);
        Assert.False(createdAtProp.IsUpdatable);

        var lastLoginProp = metadata.Properties.First(p => p.PropertyName == "LastLoginAt");
        Assert.False(lastLoginProp.IsInsertable);
        Assert.False(lastLoginProp.IsUpdatable);
        Assert.True(lastLoginProp.IsSelectable);
    }

    [Fact]
    public void CacheIsThreadSafe()
    {
        TypeMetadataCache.Clear();

        var results = new EntityMetadata?[100];

        Parallel.For(0, 100, i =>
        {
            results[i] = TypeMetadataCache.GetOrCreate<Customer>();
        });

        var first = results[0];
        Assert.All(results, r => Assert.Same(first, r));
    }
}
