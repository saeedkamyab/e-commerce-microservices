using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class ProductSpecificationValueTests
{
    [Fact]
    public void CreateText_Should_Create_Text_Value()
    {
        var result =
            ProductSpecificationValue.CreateText(
                "Samsung");

        Assert.Equal(
            AttributeType.Text,
            result.Type);

        Assert.Equal(
            "Samsung",
            result.Value);
    }
    [Fact]
    public void CreateText_Should_Trim_Value()
    {
        var result =
            ProductSpecificationValue.CreateText(
                "  Samsung  ");

        Assert.Equal(
            "Samsung",
            result.Value);
    }
    [Fact]
    public void CreateText_Should_Throw_When_Empty()
    {
        var action = () =>
            ProductSpecificationValue.CreateText("");

        Assert.Throws<ArgumentException>(action);
    }
    [Fact]
    public void CreateNumber_Should_Create_Number_Value()
    {
        var result =
            ProductSpecificationValue.CreateNumber(12);

        Assert.Equal(
            AttributeType.Number,
            result.Type);

        Assert.Equal(
            12,
            result.Value);
    }
    [Fact]
    public void CreateDecimal_Should_Create_Decimal_Value()
    {
        var result =
            ProductSpecificationValue.CreateDecimal(12.5m);

        Assert.Equal(
            AttributeType.Decimal,
            result.Type);

        Assert.Equal(
            12.5m,
            result.Value);
    }
    [Fact]
    public void CreateBoolean_Should_Create_Boolean_Value()
    {
        var result =
            ProductSpecificationValue.CreateBoolean(true);

        Assert.Equal(
            AttributeType.Boolean,
            result.Type);

        Assert.Equal(
            true,
            result.Value);
    }
    [Fact]
    public void CreateDate_Should_Create_Date_Value()
    {
        var date = new DateTime(2026, 8, 16);

        var result =
            ProductSpecificationValue.CreateDate(date);

        Assert.Equal(
            AttributeType.Date,
            result.Type);

        Assert.Equal(
            date,
            result.Value);
    }
    [Fact]
    public void CreateOption_Should_Create_Option_Value()
    {
        var option =
            AttributeOption.Create("Black");

        var result =
            ProductSpecificationValue.CreateOption(option);

        Assert.Equal(
            AttributeType.Option,
            result.Type);

        Assert.Equal(
            option,
            result.Value);
    }
}
