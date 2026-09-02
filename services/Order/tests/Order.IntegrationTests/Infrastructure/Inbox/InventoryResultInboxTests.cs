using Inventory.Contracts.IntegrationEvents;
using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Messaging;
using Order.Application.Abstractions.Persistence;
using Order.Contracts.IntegrationEvents;
using Order.Domain.Entities;
using Order.Domain.Enums;
using Order.Infrastructure.Messaging;
using Order.Infrastructure.Messaging.Inventory;
using Order.Infrastructure.Messaging.Payment;
using Order.Infrastructure.Persistence.Inbox;
using Order.Infrastructure.Persistence.Repositories;
using Payment.Contracts.IntegrationEvents;
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

    [Fact]
    public async Task Handle_Should_Mark_Order_As_InventoryReserved_And_Create_PaymentRequested_Outbox_Message()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var order = Domain.Entities.Order.Create(
            Guid.NewGuid(),
            new[]
            {
                 OrderItem.Create(
                    Guid.NewGuid(),
                    2,
                    1200m)
            });

        order.StartInventoryReservation();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        // Outbox مربوط به StartInventoryReservation را پاک می‌کنیم
        // چون در این تست فقط PaymentRequested برایمان مهم است.
        dbContext.OutboxMessages.RemoveRange(
            dbContext.OutboxMessages);

        await dbContext.SaveChangesAsync();

        var repository =
            new OrderRepository(dbContext);

        var handler =
            new InventoryReservedIntegrationEventHandler(
                repository,
                dbContext);

        var integrationEvent =
            new InventoryReservedIntegrationEvent(
                Guid.NewGuid(),
                order.Id,
                DateTime.UtcNow);

        var content =
            JsonSerializer.Serialize(integrationEvent);

        // Act
        await handler.HandleAsync(
            integrationEvent.MessageId,
            typeof(InventoryReservedIntegrationEvent).FullName!,
            content,
            CancellationToken.None);

        // Assert
        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionContext.Orders
                .AsNoTracking()
                .SingleAsync(x => x.Id == order.Id);

        Assert.Equal(
            OrderStatus.InventoryReserved,
            persistedOrder.Status);

        var outboxMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Type ==
                    typeof(PaymentRequestedIntegrationEvent).FullName);

        var paymentRequested =
            JsonSerializer.Deserialize<
                PaymentRequestedIntegrationEvent>(
                    outboxMessage.Content);

        Assert.NotNull(paymentRequested);

        Assert.Equal(
            order.Id,
            paymentRequested!.OrderId);

        Assert.Equal(
            order.TotalAmount,
            paymentRequested.Amount);

        Assert.Null(
            outboxMessage.ProcessedOnUtc);
    }

    [Fact]
    public async Task ProcessAsync_When_Payment_Fails_Should_Mark_Order_As_PaymentFailed_And_Create_ReleaseInventoryRequested_Outbox_Message()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.InboxMessages.ExecuteDeleteAsync();
        await dbContext.OutboxMessages.ExecuteDeleteAsync();
        await dbContext.OrderItems.ExecuteDeleteAsync();
        await dbContext.Orders.ExecuteDeleteAsync();

        var productId = Guid.NewGuid();

        var orderItem =
            Order.Domain.Entities.OrderItem.Create(
                productId,
                quantity: 2,
                unitPrice: 1200m);

        var order =
            Order.Domain.Entities.Order.Create(
                Guid.NewGuid(),
                [orderItem]);

        order.StartInventoryReservation();

        await dbContext.Orders.AddAsync(order);

        await dbContext.SaveChangesAsync();

  
        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        order.MarkInventoryReserved();

        await dbContext.SaveChangesAsync();

       
        await dbContext.OutboxMessages.ExecuteDeleteAsync();

        var integrationEvent =
            new PaymentFailedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                order.Id,
                "Card was declined.",
                DateTime.UtcNow);

        var handler =
            new PaymentFailedIntegrationEventHandler(
                new OrderRepository(dbContext),
                dbContext);

        var dispatcher =
            new IntegrationEventDispatcher(
                new IIntegrationEventHandler[]
                {
                    handler
                });

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var content =
            JsonSerializer.Serialize(
                integrationEvent);

        // Act
        var processed =
            await inboxProcessor.ProcessAsync(
                integrationEvent.MessageId,
                typeof(PaymentFailedIntegrationEvent).FullName!,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(processed);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionContext.Orders
                .AsNoTracking()
                .Include(x => x.Items)
                .SingleAsync(x => x.Id == order.Id);

        Assert.Equal(
            OrderStatus.PaymentFailed,
            persistedOrder.Status);

        var inboxMessage =
            await assertionContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.MessageId ==
                    integrationEvent.MessageId);

        Assert.NotNull(
            inboxMessage.ProcessedOnUtc);

        Assert.Null(
            inboxMessage.Error);

        var outboxMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Type ==
                    typeof(
                        ReleaseInventoryRequestedIntegrationEvent)
                    .FullName);

        Assert.Null(
            outboxMessage.ProcessedOnUtc);

        Assert.Null(
            outboxMessage.Error);

        var releaseEvent =
            JsonSerializer.Deserialize<
                ReleaseInventoryRequestedIntegrationEvent>(
                outboxMessage.Content);

        Assert.NotNull(releaseEvent);

        Assert.Equal(
            order.Id,
            releaseEvent.OrderId);

        Assert.Single(
            releaseEvent.Items);

        var releasedItem =
            releaseEvent.Items.Single();

        Assert.Equal(
            productId,
            releasedItem.ProductId);

        Assert.Equal(
            2,
            releasedItem.Quantity);
    }

    [Fact]
    public async Task ProcessAsync_When_Inventory_Is_Released_After_Payment_Failure_Should_Cancel_Order()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.InboxMessages
            .ExecuteDeleteAsync();

        await dbContext.OutboxMessages
            .ExecuteDeleteAsync();

        var productId = Guid.NewGuid();

        var orderItem =
            Order.Domain.Entities.OrderItem.Create(
                productId,
                quantity: 2,
                unitPrice: 1200m);

        var order =
            Order.Domain.Entities.Order.Create(
                Guid.NewGuid(),
                [orderItem]);

      
        order.StartInventoryReservation();

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

      
        await dbContext.OutboxMessages
            .ExecuteDeleteAsync();

        order.MarkInventoryReserved();

        await dbContext.SaveChangesAsync();

       
        await dbContext.OutboxMessages
            .ExecuteDeleteAsync();

        order.MarkPaymentFailed();

        await dbContext.SaveChangesAsync();


        await dbContext.OutboxMessages
            .ExecuteDeleteAsync();

        Assert.Equal(
            OrderStatus.PaymentFailed,
            order.Status);

        var integrationEvent =
            new InventoryReleasedIntegrationEvent(
                Guid.NewGuid(),
                order.Id,
                DateTime.UtcNow);

        var handler =
            new InventoryReleasedIntegrationEventHandler(
                new OrderRepository(dbContext),
                dbContext);

        IIntegrationEventHandler[] handlers =
        [
            handler
        ];

        var dispatcher =
            new IntegrationEventDispatcher(
                handlers);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var content =
            JsonSerializer.Serialize(
                integrationEvent);

        // Act
        var processed =
            await inboxProcessor.ProcessAsync(
                integrationEvent.MessageId,
                typeof(InventoryReleasedIntegrationEvent).FullName!,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(processed);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedOrder =
            await assertionContext.Orders
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == order.Id);

        Assert.Equal(
            OrderStatus.Cancelled,
            persistedOrder.Status);

        var inboxMessage =
            await assertionContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.MessageId ==
                    integrationEvent.MessageId);

        Assert.NotNull(
            inboxMessage.ProcessedOnUtc);

        Assert.Null(
            inboxMessage.Error);
    }

}
