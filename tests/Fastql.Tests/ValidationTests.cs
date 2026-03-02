using Fastql.Validation;
using Xunit;

namespace Fastql.Tests;

public class ValidationTests
{
    [Fact]
    public void Validate_ValidEntity_ReturnsValid()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = "User theme preference",
            IsActive = true
        };

        var result = EntityValidator.Validate(setting);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Validate_RequiredNull_ReturnsError()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = null!,
            IsActive = true
        };

        var result = EntityValidator.Validate(setting);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.Message.Contains("required"));
    }

    [Fact]
    public void Validate_RequiredWhitespace_ReturnsError()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = "   ",
            IsActive = true
        };

        var result = EntityValidator.Validate(setting);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.Message.Contains("required"));
    }

    [Fact]
    public void Validate_MaxLengthExceeded_ReturnsError()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = new string('x', 101), // MaxLength is 100
            IsActive = true
        };

        var result = EntityValidator.Validate(setting);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "Description" && e.Message.Contains("100"));
    }

    [Fact]
    public void Validate_MaxLengthExactly_ReturnsValid()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = new string('x', 100),
            IsActive = true
        };

        var result = EntityValidator.Validate(setting);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateProperty_SingleProperty_ReturnsResult()
    {
        var setting = new Setting
        {
            Id = 1,
            Key = "theme",
            Value = "dark",
            Description = null!,
            IsActive = true
        };

        var result = EntityValidator.ValidateProperty(setting, "Description");

        Assert.False(result.IsValid);
        Assert.Single(result.Errors);
        Assert.Equal("Description", result.Errors[0].PropertyName);
    }

    [Fact]
    public void ValidateProperty_NonExistentProperty_ReturnsValid()
    {
        var setting = new Setting { Description = "Valid" };

        var result = EntityValidator.ValidateProperty(setting, "NonExistent");

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_EntityWithNoValidationAttributes_ReturnsValid()
    {
        var customer = new Customer { Name = "Test", Email = "test@example.com" };

        var result = EntityValidator.Validate(customer);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void FastqlBuilder_Validate_ExposesValidation()
    {
        var builder = new FastqlBuilder<Setting>();
        var setting = new Setting
        {
            Description = null!
        };

        var result = builder.Validate(setting);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void FastqlHelper_Validate_ExposesValidation()
    {
        var setting = new Setting
        {
            Description = null!
        };

        var result = FastqlHelper<Setting>.Validate(setting);

        Assert.False(result.IsValid);
    }
}
