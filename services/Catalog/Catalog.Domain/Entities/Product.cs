using Catalog.Domain.Enums;
using Catalog.Domain.Events;
using Catalog.Domain.ValueObjects;
using SharedKernel.Domain;

namespace Catalog.Domain.Entities;

public sealed class Product
{
    private readonly List<IDomainEvent> _domainEvents = new();
    private readonly List<ProductSpecification> _specifications = new();

    public Guid Id { get; private set; } 
    public ProductName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Price Price { get; private set; } = null!;
    public ProductStatus Status { get; private set; }

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;

    public IReadOnlyCollection<ProductSpecification> Specifications =>
        _specifications.AsReadOnly();

    private Product()
    {
        // For EF Core
    }

    private Product(
        Guid id,
        ProductName name,
        string? description,
        Guid categoryId,
        Price price)
    {
        Id = id;
        Name = name;
        Description = description;
        CategoryId = categoryId;
        Price = price;
        Status = ProductStatus.Draft;

    }
    public static Product Create(ProductName name,string? description,
       Guid categoryId,Price price)
    {
        if (categoryId == Guid.Empty)
            throw new ArgumentException(
                "Category is required.",
                nameof(categoryId));

        var product = new Product(
            Guid.NewGuid(),
            name,
            description,
            categoryId,
            price);

        return product;
    }
    public void ChangePrice(Price newPrice)
    {
        ArgumentNullException.ThrowIfNull(newPrice);

        if (Price == newPrice)
            return;

        var oldPrice = Price;

        Price = newPrice;

        _domainEvents.Add(
            new ProductPriceChangedDomainEvent(
                Id,
                oldPrice.Amount,
                newPrice.Amount,
                newPrice.Currency,
                DateTime.UtcNow));
    }
    public void Activate()
    {
        if (Status == ProductStatus.Active)
            return;

        if (Price.Amount <= 0)
            throw new InvalidOperationException(
                "A product must have a valid price before activation.");

        Status = ProductStatus.Active;

        _domainEvents.Add(
      new ProductActivatedDomainEvent(
          Id,
          DateTime.UtcNow));
    }
    public void Deactivate()
    {
        if (Status == ProductStatus.Draft)
            throw new InvalidOperationException(
                "A draft product cannot be deactivated.");

        if (Status == ProductStatus.Inactive)
            return;

        Status = ProductStatus.Inactive;
    }
    public void AddSpecification(ProductSpecification specification)
    {
        ArgumentNullException.ThrowIfNull(specification);

        if (_specifications.Any(
            x => x.AttributeDefinitionId ==
                 specification.AttributeDefinitionId))
        {
            throw new InvalidOperationException(
                "A specification for this attribute already exists.");
        }

        _specifications.Add(specification);
    }

    public void ChangeSpecification(
    Guid attributeDefinitionId,
    ProductSpecificationValue newValue)
    {
        var specification = _specifications
            .FirstOrDefault(x =>
                x.AttributeDefinitionId == attributeDefinitionId);

        if (specification is null)
        {
            throw new InvalidOperationException(
                "Specification does not exist.");
        }

        specification.ChangeValue(newValue);
    }
    public void RemoveSpecification(
    Guid attributeDefinitionId)
    {
        var specification = _specifications
            .FirstOrDefault(x =>
                x.AttributeDefinitionId == attributeDefinitionId);

        if (specification is null)
            return;

        _specifications.Remove(specification);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}

