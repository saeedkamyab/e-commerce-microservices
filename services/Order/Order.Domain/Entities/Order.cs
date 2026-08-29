using Order.Domain.Enums;
using Order.Domain.Events;
using SharedKernel.Domain;

namespace Order.Domain.Entities;

public sealed class Order
{
    private readonly List<OrderItem> _items = new();
    private readonly List<IDomainEvent> _domainEvents = new();
    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public OrderStatus Status { get; private set; }

    public IReadOnlyCollection<OrderItem> Items =>
        _items.AsReadOnly();
    public IReadOnlyCollection<IDomainEvent> DomainEvents =>
    _domainEvents.AsReadOnly();

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
        {
            throw new InvalidOperationException(
                "Inventory reservation can only start for a pending order.");
        }

        Status = OrderStatus.AwaitingInventory;

        var items = _items
            .Select(x =>
                new InventoryReservationItem(
                    x.ProductId,
                    x.Quantity))
            .ToArray();

        _domainEvents.Add(
            new OrderInventoryReservationStartedDomainEvent(
                Id,
                items,
                DateTime.UtcNow));
    }

    public void MarkInventoryReserved()
    {
        if (Status != OrderStatus.AwaitingInventory)
            throw new InvalidOperationException(
                "Order is not waiting for inventory.");

        Status = OrderStatus.InventoryReserved;

        _domainEvents.Add(
        new OrderPaymentRequestedDomainEvent(
            Id,
            TotalAmount,
            DateTime.UtcNow));
    }

    public void MarkInventoryFailed()
    {
        if (Status != OrderStatus.AwaitingInventory)
            throw new InvalidOperationException(
                "Order is not waiting for inventory.");

        Status = OrderStatus.Cancelled;
    }
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
