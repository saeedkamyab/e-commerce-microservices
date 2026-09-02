namespace Inventory.Domain.Entities;

public sealed class InventoryItem
{
    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public int ReservedQuantity { get; private set; }

    public int AvailableQuantity =>
        Quantity - ReservedQuantity;

    public uint Version { get; private set; }

    private InventoryItem()
    {
    }

    private InventoryItem(
        Guid id,
        Guid productId)
    {
        Id = id;
        ProductId = productId;
        Quantity = 0;
        ReservedQuantity = 0;
    }

    public static InventoryItem Create(Guid productId)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product id cannot be empty.",
                nameof(productId));

        return new InventoryItem(
            Guid.NewGuid(),
            productId);
    }

    public void IncreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity));

        Quantity += quantity;
    }

    public void DecreaseStock(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                "Insufficient available stock.");

        Quantity -= quantity;
    }

    public void Reserve(int quantity)
    {
        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity));

        if (quantity > AvailableQuantity)
            throw new InvalidOperationException(
                "Insufficient available stock.");

        ReservedQuantity += quantity;
    }

    public void ReleaseReservation(int quantity)
    {
       
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(quantity),
                "Quantity must be greater than zero.");
        }

        if (ReservedQuantity < quantity)
        {
            throw new InvalidOperationException(
                "Cannot release more quantity than currently reserved.");
        }

        ReservedQuantity -= quantity;
    }
}
