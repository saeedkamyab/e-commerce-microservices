namespace Order.Domain.Entities;

public sealed class OrderItem
{
    public Guid Id { get; private set; }
    public Guid ProductId { get; private set; }

    public int Quantity { get; private set; }

    public decimal UnitPrice { get; private set; }

    private OrderItem()
    {
    }


    private OrderItem(
    Guid id,
    Guid productId,
    int quantity,
    decimal unitPrice)
    {
        Id = id;
        ProductId = productId;
        Quantity = quantity;
        UnitPrice = unitPrice;
    }
    public static OrderItem Create(
        Guid productId,
        int quantity,
        decimal unitPrice)
    {
        if (productId == Guid.Empty)
            throw new ArgumentException(
                "Product id cannot be empty.",
                nameof(productId));

        if (quantity <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantity));

        if (unitPrice <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(unitPrice));

        return new OrderItem(
            Guid.NewGuid(),
            productId,
            quantity,
            unitPrice);
    }

    public decimal Total =>
        UnitPrice * Quantity;
}
