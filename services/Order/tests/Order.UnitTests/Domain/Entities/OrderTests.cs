using Order.Domain.Entities;
using Order.Domain.Enums;

namespace Order.UnitTests.Domain.Entities;

public sealed class OrderTests
{
    [Fact]
    public void Create_Should_Create_Pending_Order()
    {
        // Arrange
        var userId = Guid.NewGuid();

        var items = new[]
        {
            OrderItem.Create(
                Guid.NewGuid(),
                2,
                100m)
        };

        // Act
        var order = Order.Domain.Entities.Order.Create(
            userId,
            items);

        // Assert
        Assert.NotEqual(Guid.Empty, order.Id);
        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Pending, order.Status);

        Assert.Single(order.Items);
        Assert.Equal(200m, order.TotalAmount);
    }

    [Fact]
    public void Create_Without_Items_Should_Throw()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var action = () =>
            Order.Domain.Entities.Order.Create(
                userId,
                Array.Empty<OrderItem>());

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void StartInventoryReservation_Should_Change_Status_To_AwaitingInventory()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        order.StartInventoryReservation();

        // Assert
        Assert.Equal(
            OrderStatus.AwaitingInventory,
            order.Status);
    }

    [Fact]
    public void MarkInventoryReserved_Should_Change_Status_To_InventoryReserved()
    {
        // Arrange
        var order = CreateOrder();

        order.StartInventoryReservation();

        // Act
        order.MarkInventoryReserved();

        // Assert
        Assert.Equal(
            OrderStatus.InventoryReserved,
            order.Status);
    }

    [Fact]
    public void MarkInventoryFailed_Should_Cancel_Order()
    {
        // Arrange
        var order = CreateOrder();

        order.StartInventoryReservation();

        // Act
        order.MarkInventoryFailed();

        // Assert
        Assert.Equal(
            OrderStatus.Cancelled,
            order.Status);
    }

    [Fact]
    public void MarkInventoryReserved_When_Order_Is_Not_AwaitingInventory_Should_Throw()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        var action = () =>
            order.MarkInventoryReserved();

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void MarkInventoryFailed_When_Order_Is_Not_AwaitingInventory_Should_Throw()
    {
        // Arrange
        var order = CreateOrder();

        // Act
        var action = () =>
            order.MarkInventoryFailed();

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    private static Order.Domain.Entities.Order CreateOrder()
    {
        var item = OrderItem.Create(
            Guid.NewGuid(),
            2,
            100m);

        return Order.Domain.Entities.Order.Create(
            Guid.NewGuid(),
            [item]);
    }
}
