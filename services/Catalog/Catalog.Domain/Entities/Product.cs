using Catalog.Domain.Enums;
using Catalog.Domain.Events;
using Catalog.Domain.ValueObjects;
using SharedKernel.Domain;

namespace Catalog.Domain.Entities;

public sealed class Product
{
    private readonly List<IDomainEvent> _domainEvents = new();
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents;


    public Guid Id { get; private set; } 
    public ProductName Name { get; private set; } = null!;
    public string? Description { get; private set; }
    public Guid CategoryId { get; private set; }
    public Price Price { get; private set; } = null!;
    public ProductStatus Status { get; private set; }
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
}

