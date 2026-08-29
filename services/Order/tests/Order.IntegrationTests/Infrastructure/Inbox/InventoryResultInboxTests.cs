using Inventory.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Persistence.Inbox;
using Order.Infrastructure.Persistence.Repositories;
using System.Text.Json;

namespace Order.IntegrationTests.Infrastructure.Inbox;


[Collection(OrderDatabaseCollection.Name)]
public sealed class InventoryResultInboxTests
{
    private readonly OrderDatabaseFixture _fixture;

    public InventoryResultInboxTests(
        OrderDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_Inventory_Is_Reserved_Should_Update_Order_And_Process_Inbox()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var items = new[]
      {
            OrderItem.Create(
                Guid.NewGuid(),
                2,
                100m)
        };

        var order =
            Order.Domain.Entities.Order.Create(
                Guid.NewGuid(),
                items);


        order.StartInventoryReservation();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        // Domain events مربوط به setup تست را پاک می‌کنیم
        order.ClearDomainEvents();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new InventoryReservedIntegrationEventHandler(
                repository,
                unitOfWork);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();

        var integrationEvent =
            new InventoryReservedIntegrationEvent(
                messageId,
                order.Id,
                DateTime.UtcNow);

        var type =
            typeof(InventoryReservedIntegrationEvent)
                .FullName!;

        var content =
            JsonSerializer.Serialize(
                integrationEvent);

        // Act
        var result =
            await inboxProcessor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(result);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionDbContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

        Assert.Equal(
            OrderStatus.InventoryReserved,
            persistedOrder.Status);

        var inboxMessage =
            await assertionDbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.MessageId == messageId);

        Assert.NotNull(
            inboxMessage.ProcessedOnUtc);

        Assert.Null(
            inboxMessage.Error);
    }
    [Fact]
    public async Task ProcessAsync_When_Inventory_Reservation_Fails_Should_Cancel_Order_And_Process_Inbox()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var item = OrderItem.Create(
            Guid.NewGuid(),
            1,
            100m);

        var order =
            Order.Domain.Entities.Order.Create(
                Guid.NewGuid(),
                [item]);

        order.StartInventoryReservation();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        order.ClearDomainEvents();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new InventoryReservationFailedIntegrationEventHandler(
                repository,
                unitOfWork);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();

        var integrationEvent =
            new InventoryReservationFailedIntegrationEvent(
                messageId,
                order.Id,
                "Insufficient stock.",
                DateTime.UtcNow);

        var type =
            typeof(InventoryReservationFailedIntegrationEvent)
                .FullName!;

        var content =
            JsonSerializer.Serialize(integrationEvent);

        // Act
        var result =
            await inboxProcessor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(result);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionDbContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

        Assert.Equal(
            OrderStatus.Cancelled,
            persistedOrder.Status);

        var inboxMessage =
            await assertionDbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.MessageId == messageId);

        Assert.NotNull(inboxMessage.ProcessedOnUtc);
        Assert.Null(inboxMessage.Error);
    }
    [Fact]
    public async Task ProcessAsync_With_Same_Message_Twice_Should_Process_Only_Once()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var item = OrderItem.Create(
            Guid.NewGuid(),
            1,
            100m);

        var order =
            Order.Domain.Entities.Order.Create(
                Guid.NewGuid(),
                [item]);

        order.StartInventoryReservation();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        order.ClearDomainEvents();

        var repository =
            new OrderRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new InventoryReservedIntegrationEventHandler(
                repository,
                unitOfWork);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();

        var integrationEvent =
            new InventoryReservedIntegrationEvent(
                messageId,
                order.Id,
                DateTime.UtcNow);

        var type =
            typeof(InventoryReservedIntegrationEvent)
                .FullName!;

        var content =
            JsonSerializer.Serialize(integrationEvent);

        // Act
        var firstResult =
            await inboxProcessor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        var secondResult =
            await inboxProcessor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(firstResult);
        Assert.False(secondResult);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionDbContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

        Assert.Equal(
            OrderStatus.InventoryReserved,
            persistedOrder.Status);

        var inboxCount =
            await assertionDbContext.InboxMessages
                .CountAsync(x =>
                    x.MessageId == messageId);

        Assert.Equal(1, inboxCount);
    }
}