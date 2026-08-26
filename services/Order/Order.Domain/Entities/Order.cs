using Order.Domain.Enums;

namespace Order.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items =>
        _items.AsReadOnly();

    public decimal TotalAmount =>
        _items.Sum(x => x.Total);

    private Order()
    {
    }

    private Order(
        Guid id,
        Guid userId,
        IEnumerable<OrderItem> items)
    {
        Id = id;
        UserId = userId;
        _items.AddRange(items);
        Status = OrderStatus.Pending;
    }

    public static Order Create(
        Guid userId,
        IEnumerable<OrderItem> items)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException(
                "User id cannot be empty.",
                nameof(userId));

        var orderItems = items.ToList();

        if (orderItems.Count == 0)
            throw new InvalidOperationException(
                "Order must contain at least one item.");

        return new Order(
            Guid.NewGuid(),
            userId,
            orderItems);
    }

    public void StartInventoryReservation()
    {
        if (Status != OrderStatus.Pending)
            throw new InvalidOperationException(
                "Inventory reservation can only start for a pending order.");

        Status = OrderStatus.AwaitingInventory;
    }

    public void MarkInventoryReserved()
    {
        if (Status != OrderStatus.AwaitingInventory)
            throw new InvalidOperationException(
                "Order is not waiting for inventory.");

        Status = OrderStatus.InventoryReserved;
    }

    public void MarkInventoryFailed()
    {
        if (Status != OrderStatus.AwaitingInventory)
            throw new InvalidOperationException(
                "Order is not waiting for inventory.");

        Status = OrderStatus.Cancelled;
    }
}
