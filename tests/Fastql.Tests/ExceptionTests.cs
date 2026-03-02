using Fastql.Exceptions;
using Xunit;

namespace Fastql.Tests;

public class ExceptionTests
{
    [Fact]
    public void FastQueryBuilder_InsertSql_ThrowsMissingParametersException_WhenEmpty()
    {
        var qb = new FastQueryBuilder("TestTable");

        Assert.Throws<MissingParametersException>(() => _ = qb.InsertSql);
    }

    [Fact]
    public void FastQueryBuilder_InsertReturnObjectSql_ThrowsMissingParametersException_WhenEmpty()
    {
        var qb = new FastQueryBuilder("TestTable");

        Assert.Throws<MissingParametersException>(() => _ = qb.InsertReturnObjectSql);
    }

    [Fact]
    public void FastQueryBuilder_UpdateSql_ThrowsMissingWhereClauseException_WhenNoWhere()
    {
        var qb = new FastQueryBuilder("TestTable");
        qb.Add("Name", "Name", "test");

        Assert.Throws<MissingWhereClauseException>(() => _ = qb.UpdateSql);
    }

    [Fact]
    public void FastQueryBuilder_UpdateSql_ThrowsMissingParametersException_WhenEmpty()
    {
        var qb = new FastQueryBuilder("TestTable", " WHERE Id = 1");

        Assert.Throws<MissingParametersException>(() => _ = qb.UpdateSql);
    }

    [Fact]
    public void FastQueryBuilder_SelectSql_ThrowsMissingWhereClauseException_WhenNoWhere()
    {
        var qb = new FastQueryBuilder("TestTable");
        qb.Add("Name", "Name", "test");

        Assert.Throws<MissingWhereClauseException>(() => _ = qb.SelectSql);
    }

    [Fact]
    public void FastQueryBuilder_Add_ThrowsDuplicateFieldException_WhenDuplicate()
    {
        var qb = new FastQueryBuilder("TestTable");
        qb.Add("Name", "Name", "test");

        var ex = Assert.Throws<DuplicateFieldException>(() => qb.Add("Name", "Name", "test2"));
        Assert.Equal("Name", ex.FieldName);
    }

    [Fact]
    public void FastQueryBuilder_ReturnStatement_ThrowsMissingParametersException_WhenEmpty()
    {
        var qb = new FastQueryBuilder("TestTable");

        Assert.Throws<MissingParametersException>(() => _ = qb.ReturnStatement);
    }

    [Fact]
    public void MissingParametersException_HasDefaultMessage()
    {
        var ex = new MissingParametersException();
        Assert.Contains("No properties to include in query", ex.Message);
    }

    [Fact]
    public void MissingWhereClauseException_HasDefaultMessage()
    {
        var ex = new MissingWhereClauseException();
        Assert.Contains("WHERE clause is required", ex.Message);
    }

    [Fact]
    public void DuplicateFieldException_IncludesFieldName()
    {
        var ex = new DuplicateFieldException("MyField");
        Assert.Contains("MyField", ex.Message);
        Assert.Equal("MyField", ex.FieldName);
    }

    [Fact]
    public void AllCustomExceptions_InheritFromFastqlException()
    {
        Assert.IsAssignableFrom<FastqlException>(new MissingParametersException());
        Assert.IsAssignableFrom<FastqlException>(new MissingWhereClauseException());
        Assert.IsAssignableFrom<FastqlException>(new DuplicateFieldException("test"));
    }
}
