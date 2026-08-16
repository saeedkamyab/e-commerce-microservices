using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;

namespace Catalog.UnitTests.Domain.Entities;

public class ProductSpecificationTest
{
    [Fact]
    public void Create_Should_Create_Specification()
    {
        var attributeDefinitionId =
            Guid.NewGuid();

        var value =
            ProductSpecificationValue.CreateNumber(8);

        var specification =
            ProductSpecification.Create(
                attributeDefinitionId,
                value);

        Assert.NotEqual(
            Guid.Empty,
            specification.Id);

        Assert.Equal(
            attributeDefinitionId,
            specification.AttributeDefinitionId);

        Assert.Equal(
            AttributeType.Number,
            specification.Value.Type);

        Assert.Equal(
            8,
            specification.Value.Value);
    }
}
