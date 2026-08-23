using Catalog.Domain.ValueObjects;

namespace Catalog.UnitTests.Domain.ValueObjects;

public class CategoryNameTests
{
    [Fact]
    public void Create_Should_Create_CategoryName()
    {
        // Act
        var name = CategoryName.Create("Electronics");

        // Assert
        Assert.Equal("Electronics", name.Value);
    }

    [Fact]
    public void Create_Should_Trim_Whitespace()
    {
        // Act
        var name = CategoryName.Create("  Electronics  ");

        // Assert
        Assert.Equal("Electronics", name.Value);
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Empty()
    {
        // Act
        var action = () => CategoryName.Create("");
      

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Is_Whitespace()
    {
        // Act
        var action = () => CategoryName.Create("   ");

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Create_Should_Throw_When_Name_Exceeds_Maximum_Length()
    {
        // Arrange
        var value = new string('A', 101);

        // Act
        var action = () => CategoryName.Create(value);

        // Assert
        Assert.Throws<ArgumentException>(action);
    }

    [Fact]
    public void Two_CategoryNames_With_Same_Value_Should_Be_Equal()
    {
        // Arrange
        var first = CategoryName.Create("Electronics");
        var second = CategoryName.Create("Electronics");

        // Assert
        Assert.Equal(first, second);
    }
}
