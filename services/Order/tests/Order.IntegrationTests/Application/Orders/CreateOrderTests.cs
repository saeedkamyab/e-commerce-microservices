using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Domain.Enums;
using Order.Infrastructure.Persistence.Repositories;
using Order.IntegrationTests.Infrastructure;

namespace Order.IntegrationTests.Application.Orders;

[Collection(OrderDatabaseCollection.Name)]
public sealed class CreateOrderTests
{
    private readonly OrderDatabaseFixture _fixture;

    public CreateOrderTests(
        OrderDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Order_With_Items()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new CreateOrderCommandHandler(
                repository,
                unitOfWork);

        var userId = Guid.NewGuid();

        var product1Id = Guid.NewGuid();
        var product2Id = Guid.NewGuid();

        var command =
            new CreateOrderCommand(
                userId,
                [
                    new CreateOrderItemInput(
                        product1Id,
                        2,
                        100m),

                    new CreateOrderItemInput(
                        product2Id,
                        1,
                        250m)
                ]);

        // Act
        var orderId =
            await handler.Handle(
                command,
                CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var order =
            await assertionDbContext.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .SingleAsync(x => x.Id == orderId);

        Assert.Equal(userId, order.UserId);
        Assert.Equal(OrderStatus.Pending, order.Status);

        Assert.Equal(2, order.Items.Count);

        Assert.Equal(
            450m,
            order.TotalAmount);
    }
}
