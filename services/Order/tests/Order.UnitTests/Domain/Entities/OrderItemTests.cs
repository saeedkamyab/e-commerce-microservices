using Order.Domain.Entities;

namespace Order.UnitTests.Domain.Entities;

public sealed class OrderItemTests
{
    [Fact]
    public void Create_Should_Calculate_Total()
    {
        var item = OrderItem.Create(
            Guid.NewGuid(),
            3,
            150m);

        Assert.Equal(3, item.Quantity);
        Assert.Equal(150m, item.UnitPrice);
        Assert.Equal(450m, item.Total);
    }

    [Fact]
    public void Create_With_Empty_ProductId_Should_Throw()
    {
        var action = () =>
            OrderItem.Create(
                Guid.Empty,
                1,
                100m);

        Assert.Throws<ArgumentException>(action);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Create_With_Invalid_Quantity_Should_Throw(
        int quantity)
    {
        var action = () =>
            OrderItem.Create(
                Guid.NewGuid(),
                quantity,
                100m);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }

    [Fact]
    public void Create_With_Invalid_UnitPrice_Should_Throw()
    {
        var action = () =>
            OrderItem.Create(
                Guid.NewGuid(),
                1,
                0m);

        Assert.Throws<ArgumentOutOfRangeException>(action);
    }
}
