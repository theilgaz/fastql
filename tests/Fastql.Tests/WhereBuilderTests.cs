using Xunit;
using WhereBuilder = Fastql.Where.Where;

namespace Fastql.Tests;

public class WhereBuilderTests
{
    [Fact]
    public void SimpleEquals_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Id").Equals("@Id").Build();

        Assert.Equal("Id = @Id", result);
    }

    [Fact]
    public void SimpleNotEquals_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Status").NotEquals("@Status").Build();

        Assert.Equal("Status <> @Status", result);
    }

    [Fact]
    public void GreaterThan_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Age").GreaterThan("@Age").Build();

        Assert.Equal("Age > @Age", result);
    }

    [Fact]
    public void LessThan_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Price").LessThan("@MaxPrice").Build();

        Assert.Equal("Price < @MaxPrice", result);
    }

    [Fact]
    public void Like_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Name").Like("@Pattern").Build();

        Assert.Equal("Name LIKE @Pattern", result);
    }

    [Fact]
    public void In_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Status").In("@Statuses").Build();

        Assert.Equal("Status IN (@Statuses)", result);
    }

    [Fact]
    public void IsNull_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("DeletedAt").IsNull().Build();

        Assert.Equal("DeletedAt IS NULL", result);
    }

    [Fact]
    public void IsNotNull_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Email").IsNotNull().Build();

        Assert.Equal("Email IS NOT NULL", result);
    }

    [Fact]
    public void Between_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Age").Between("@MinAge", "@MaxAge").Build();

        Assert.Equal("Age BETWEEN @MinAge AND @MaxAge", result);
    }

    [Fact]
    public void AndChaining_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Id").Equals("@Id")
            .And.Column("Active").Equals("@Active")
            .Build();

        Assert.Equal("Id = @Id AND Active = @Active", result);
    }

    [Fact]
    public void OrChaining_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Status").Equals("@Status1")
            .Or.Column("Status").Equals("@Status2")
            .Build();

        Assert.Equal("Status = @Status1 OR Status = @Status2", result);
    }

    [Fact]
    public void ComplexExpression_MultipleAndOr()
    {
        string result = WhereBuilder.Column("Id").GreaterThan("@MinId")
            .And.Column("Active").Equals("@Active")
            .Or.Column("Role").Equals("@AdminRole")
            .Build();

        Assert.Equal("Id > @MinId AND Active = @Active OR Role = @AdminRole", result);
    }

    [Fact]
    public void ImplicitStringConversion_Works()
    {
        string where = WhereBuilder.Column("Id").Equals("@Id");

        Assert.Equal("Id = @Id", where);
    }

    [Fact]
    public void ToString_ReturnsExpression()
    {
        var expression = WhereBuilder.Column("Id").Equals("@Id");

        Assert.Equal("Id = @Id", expression.ToString());
    }

    [Fact]
    public void WhereBuilder_CanBeUsedWithSelectQuery()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        string where = WhereBuilder.Column("Id").Equals("@Id");
        var query = builder.SelectQuery(where);

        Assert.Contains("WHERE Id = @Id", query);
    }

    [Fact]
    public void WhereBuilder_CanBeUsedWithDeleteQuery()
    {
        var builder = new FastqlBuilder<Customer>(DatabaseType.SqlServer);
        string where = WhereBuilder.Column("Id").Equals("@Id").And.Column("Active").Equals("@Active");
        var query = builder.DeleteQuery(where);

        Assert.Contains("WHERE Id = @Id AND Active = @Active", query);
    }

    [Fact]
    public void GreaterThanOrEqual_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Age").GreaterThanOrEqual("@MinAge").Build();

        Assert.Equal("Age >= @MinAge", result);
    }

    [Fact]
    public void LessThanOrEqual_GeneratesCorrectClause()
    {
        string result = WhereBuilder.Column("Age").LessThanOrEqual("@MaxAge").Build();

        Assert.Equal("Age <= @MaxAge", result);
    }
}
