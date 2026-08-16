using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.UnitTests.Domain.Entities;

public class CategoryAttributeDefinitionTests
{
    [Fact]
    public void Create_Should_Create_Attribute_Definition()
    {
        var attribute =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        Assert.NotEqual(Guid.Empty, attribute.Id);
        Assert.Equal("RAM", attribute.Name);
        Assert.Equal(AttributeType.Number, attribute.Type);
        Assert.True(attribute.IsRequired);
    }
    [Fact]
    public void AddOption_Should_Add_Option_To_Option_Attribute()
    {
        var attribute =
            CategoryAttributeDefinition.Create(
                "Color",
                AttributeType.Option,
                true);

        var option =
            AttributeOption.Create("Black");

        attribute.AddOption(option);

        var result = Assert.Single(attribute.Options);

        Assert.Equal(option, result);
    }
    [Fact]
    public void AddOption_Should_Throw_For_NonOption_Attribute()
    {
        var attribute =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        var option =
            AttributeOption.Create("8 GB");

        var action = () => attribute.AddOption(option);

        Assert.Throws<InvalidOperationException>(action);
    }
    [Fact]
    public void AddOption_Should_Not_Add_Duplicate_Option()
    {
        var attribute =
            CategoryAttributeDefinition.Create(
                "Color",
                AttributeType.Option,
                true);

        attribute.AddOption(
            AttributeOption.Create("Black"));

        attribute.AddOption(
            AttributeOption.Create("Black"));

        Assert.Single(attribute.Options);
    }
}
