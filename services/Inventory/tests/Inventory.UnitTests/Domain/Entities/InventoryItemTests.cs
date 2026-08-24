using Inventory.Domain.Entities;

namespace Inventory.UnitTests.Domain.Entities;

public sealed class InventoryItemTests
{
    [Fact]
    public void Create_Should_Create_Empty_Inventory()
    {
        // Arrange
        var productId = Guid.NewGuid();

        // Act
        var inventoryItem =
            InventoryItem.Create(productId);

        // Assert
        Assert.NotEqual(Guid.Empty, inventoryItem.Id);
        Assert.Equal(productId, inventoryItem.ProductId);

        Assert.Equal(0, inventoryItem.Quantity);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(0, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void IncreaseStock_Should_Increase_Quantity()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);

        Assert.Equal(10, inventoryItem.Quantity);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(10, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void Reserve_Should_Increase_ReservedQuantity()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);

        inventoryItem.Reserve(4);

        Assert.Equal(10, inventoryItem.Quantity);
        Assert.Equal(4, inventoryItem.ReservedQuantity);
        Assert.Equal(6, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void Reserve_When_AvailableStock_Is_Insufficient_Should_Throw()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(5);

        var action = () =>
            inventoryItem.Reserve(6);

        Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(5, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void ReleaseReservation_Should_Decrease_ReservedQuantity()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);
        inventoryItem.Reserve(6);

        inventoryItem.ReleaseReservation(2);

        Assert.Equal(10, inventoryItem.Quantity);
        Assert.Equal(4, inventoryItem.ReservedQuantity);
        Assert.Equal(6, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void ReleaseReservation_When_Quantity_Exceeds_Reserved_Should_Throw()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);
        inventoryItem.Reserve(3);

        var action = () =>
            inventoryItem.ReleaseReservation(4);

        Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(3, inventoryItem.ReservedQuantity);
    }

    [Fact]
    public void DecreaseStock_Should_Decrease_Quantity()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);

        inventoryItem.DecreaseStock(3);

        Assert.Equal(7, inventoryItem.Quantity);
        Assert.Equal(7, inventoryItem.AvailableQuantity);
    }

    [Fact]
    public void DecreaseStock_Should_Not_Use_Reserved_Stock()
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        inventoryItem.IncreaseStock(10);
        inventoryItem.Reserve(7);

        // Available = 3
        var action = () =>
            inventoryItem.DecreaseStock(4);

        Assert.Throws<InvalidOperationException>(action);

        Assert.Equal(10, inventoryItem.Quantity);
        Assert.Equal(7, inventoryItem.ReservedQuantity);
        Assert.Equal(3, inventoryItem.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10)]
    public void IncreaseStock_With_NonPositive_Quantity_Should_Throw(
        int quantity)
    {
        var inventoryItem =
            InventoryItem.Create(Guid.NewGuid());

        Assert.Throws<ArgumentOutOfRangeException>(
            () => inventoryItem.IncreaseStock(quantity));
    }

    [Fact]
    public void Create_With_Empty_ProductId_Should_Throw()
    {
        Assert.Throws<ArgumentException>(
            () => InventoryItem.Create(Guid.Empty));
    }

}