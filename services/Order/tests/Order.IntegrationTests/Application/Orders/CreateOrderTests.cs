using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Authentication;
using Order.Application.Abstractions.Persistence;
using Order.Application.Orders.Commands.CreateOrder;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Enums;
using Order.Infrastructure.Persistence.Repositories;
using Order.IntegrationTests.Infrastructure;
using System.Text.Json;

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

        IUnitOfWork unitOfWork =
            dbContext;

        var currentUserId =
            Guid.NewGuid();

        var currentUser =
            new FakeCurrentUser(
                currentUserId);

        var product1Id =
            Guid.NewGuid();

        var product2Id =
            Guid.NewGuid();

        var catalogService =
            new FakeCatalogService();

        catalogService.AddProduct(
            product1Id,
            100m);

        catalogService.AddProduct(
            product2Id,
            250m);

        var handler =
            new CreateOrderCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                catalogService);

        var command =
            new CreateOrderCommand(
                [
                    new CreateOrderItemInput(
                    product1Id,
                    2),

                new CreateOrderItemInput(
                    product2Id,
                    1)
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

        Assert.Equal(
            currentUserId,
            order.UserId);

        Assert.Equal(
            OrderStatus.AwaitingInventory,
            order.Status);

        Assert.Equal(
            2,
            order.Items.Count);

        Assert.Equal(
            450m,
            order.TotalAmount);

        var firstItem =
            Assert.Single(
                order.Items.Where(
                    x => x.ProductId == product1Id));

        Assert.Equal(
            2,
            firstItem.Quantity);

        Assert.Equal(
            100m,
            firstItem.UnitPrice);

        var secondItem =
            Assert.Single(
                order.Items.Where(
                    x => x.ProductId == product2Id));

        Assert.Equal(
            1,
            secondItem.Quantity);

        Assert.Equal(
            250m,
            secondItem.UnitPrice);

        var outboxMessages =
            await assertionDbContext.OutboxMessages
                .AsNoTracking()
                .ToListAsync();

        var outboxMessage =
            Assert.Single(
                outboxMessages.Where(
                    x =>
                        x.Type ==
                        typeof(
                            ReserveInventoryRequestedIntegrationEvent)
                        .FullName));

        Assert.NotNull(
            outboxMessage.Content);

        var integrationEvent =
            JsonSerializer
                .Deserialize<
                    ReserveInventoryRequestedIntegrationEvent>(
                    outboxMessage.Content);

        Assert.NotNull(
            integrationEvent);

        Assert.Equal(
            orderId,
            integrationEvent.OrderId);

        Assert.Equal(
            2,
            integrationEvent.Items.Count);

        Assert.Contains(
            integrationEvent.Items,
            x =>
                x.ProductId == product1Id &&
                x.Quantity == 2);

        Assert.Contains(
            integrationEvent.Items,
            x =>
                x.ProductId == product2Id &&
                x.Quantity == 1);

        Assert.Null(
            outboxMessage.ProcessedOnUtc);

        Assert.Null(
            outboxMessage.Error);
    }

    [Fact]
    public async Task Handle_Should_Not_Create_Order_When_Product_Does_Not_Exist()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork =
            dbContext;

        var currentUser =
            new FakeCurrentUser(
                Guid.NewGuid());

        var catalogService =
            new FakeCatalogService();

        var missingProductId =
            Guid.NewGuid();

        var handler =
            new CreateOrderCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                catalogService);

        var command =
            new CreateOrderCommand(
                [
                    new CreateOrderItemInput(
                    missingProductId,
                    1)
                ]);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () =>
                    handler.Handle(
                        command,
                        CancellationToken.None));

        // Assert
        Assert.Contains(
            missingProductId.ToString(),
            exception.Message);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        Assert.False(
            await assertionDbContext.Orders.AnyAsync());

        Assert.False(
            await assertionDbContext.OutboxMessages.AnyAsync());
    }


    [Fact]
    public async Task Handle_Should_Not_Create_Order_When_Product_Is_Inactive()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork =
            dbContext;

        var currentUser =
            new FakeCurrentUser(
                Guid.NewGuid());

        var productId =
            Guid.NewGuid();

        var catalogService =
            new FakeCatalogService();

        catalogService.AddProduct(
            productId,
            100m,
            isActive: false);

        var handler =
            new CreateOrderCommandHandler(
                repository,
                unitOfWork,
                currentUser,
                catalogService);

        var command =
            new CreateOrderCommand(
                [
                    new CreateOrderItemInput(
                    productId,
                    1)
                ]);

        // Act
        var exception =
            await Assert.ThrowsAsync<ArgumentException>(
                () =>
                    handler.Handle(
                        command,
                        CancellationToken.None));

        // Assert
        Assert.Contains(
            productId.ToString(),
            exception.Message);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        Assert.False(
            await assertionDbContext.Orders.AnyAsync());

        Assert.False(
            await assertionDbContext.OutboxMessages.AnyAsync());
    }

}
