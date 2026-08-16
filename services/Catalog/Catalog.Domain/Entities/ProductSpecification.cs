using Catalog.Domain.ValueObjects;

namespace Catalog.Domain.Entities;

public sealed class ProductSpecification
{
    public Guid Id { get; private set; }

    public Guid AttributeDefinitionId { get; private set; }

    public ProductSpecificationValue Value { get; private set; } = null!;

    private ProductSpecification()
    {
        // For EF Core
    }

    private ProductSpecification(
        Guid id,
        Guid attributeDefinitionId,
        ProductSpecificationValue value)
    {
        Id = id;
        AttributeDefinitionId = attributeDefinitionId;
        Value = value;
    }

    public static ProductSpecification Create(
        Guid attributeDefinitionId,
        ProductSpecificationValue value)
    {
        if (attributeDefinitionId == Guid.Empty)
            throw new ArgumentException(
                "Attribute definition id cannot be empty.",
                nameof(attributeDefinitionId));

        ArgumentNullException.ThrowIfNull(value);

        return new ProductSpecification(
            Guid.NewGuid(),
            attributeDefinitionId,
            value);
    }

    public void ChangeValue(ProductSpecificationValue value)
    {
        ArgumentNullException.ThrowIfNull(value);

        Value = value;
    }
}
