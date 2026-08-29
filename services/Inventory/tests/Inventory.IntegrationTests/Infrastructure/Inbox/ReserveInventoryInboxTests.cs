using Inventory.Application.Abstractions.Persistence;
using Inventory.Contracts.IntegrationEvents;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence.Inbox;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Order.Contracts.IntegrationEvents;
using System.Text.Json;

namespace Inventory.IntegrationTests.Infrastructure.Inbox;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class ReserveInventoryInboxTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public ReserveInventoryInboxTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_One_Product_Has_Insufficient_Stock_Should_Not_Reserve_Any_Item_And_Should_Create_Failure_Outbox_Message()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var productAId = Guid.NewGuid();
        var productBId = Guid.NewGuid();

        var productA =
            InventoryItem.Create(productAId);

        productA.IncreaseStock(10);

        var productB =
            InventoryItem.Create(productBId);

        productB.IncreaseStock(2);

        dbContext.InventoryItems.AddRange(
            productA,
            productB);

        await dbContext.SaveChangesAsync();

        var repository =
    new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new ReserveInventoryRequestedIntegrationEventHandler(
                repository,
                unitOfWork,
                dbContext);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var integrationEvent =
            new ReserveInventoryRequestedIntegrationEvent(
                messageId,
                orderId,
                [
                    new ReserveInventoryItem(
                        productAId,
                        5),

                    new ReserveInventoryItem(
                        productBId,
                        3)
                ],
                DateTime.UtcNow);

        var type =
            typeof(ReserveInventoryRequestedIntegrationEvent)
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

        Assert.True(result);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedProductA =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(x => x.ProductId == productAId);

        var persistedProductB =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(x => x.ProductId == productBId);

        Assert.Equal(0, persistedProductA.ReservedQuantity);
        Assert.Equal(0, persistedProductB.ReservedQuantity);

        Assert.Equal(10, persistedProductA.AvailableQuantity);
        Assert.Equal(2, persistedProductB.AvailableQuantity);

        var inboxMessage =
            await assertionDbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(x => x.MessageId == messageId);

        Assert.NotNull(inboxMessage.ProcessedOnUtc);
        Assert.Null(inboxMessage.Error);

  
        var outboxMessage =
            await assertionDbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Type ==
                    typeof(InventoryReservationFailedIntegrationEvent).FullName);

        Assert.Null(outboxMessage.ProcessedOnUtc);
        Assert.Null(outboxMessage.Error);

    }

    [Fact]
    public async Task ProcessAsync_When_All_Products_Have_Enough_Stock_Should_Reserve_All_And_Mark_Inbox_As_Processed()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var productAId = Guid.NewGuid();
        var productBId = Guid.NewGuid();

        var productA =
            InventoryItem.Create(productAId);

        productA.IncreaseStock(10);

        var productB =
            InventoryItem.Create(productBId);

        productB.IncreaseStock(5);

        dbContext.InventoryItems.AddRange(
            productA,
            productB);

        await dbContext.SaveChangesAsync();

        var repository =
     new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new ReserveInventoryRequestedIntegrationEventHandler(
                repository,
                unitOfWork,
                dbContext);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var integrationEvent =
            new ReserveInventoryRequestedIntegrationEvent(
                messageId,
                orderId,
                [
                    new ReserveInventoryItem(
                    productAId,
                    4),

                new ReserveInventoryItem(
                    productBId,
                    2)
                ],
                DateTime.UtcNow);

        var type =
            typeof(ReserveInventoryRequestedIntegrationEvent)
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

        var persistedProductA =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(
                    x => x.ProductId == productAId);

        var persistedProductB =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(
                    x => x.ProductId == productBId);

        Assert.Equal(
            4,
            persistedProductA.ReservedQuantity);

        Assert.Equal(
            6,
            persistedProductA.AvailableQuantity);

        Assert.Equal(
            2,
            persistedProductB.ReservedQuantity);

        Assert.Equal(
            3,
            persistedProductB.AvailableQuantity);

        var inboxMessage =
            await assertionDbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(
                    x => x.MessageId == messageId);

        Assert.NotNull(
            inboxMessage.ProcessedOnUtc);

        Assert.Null(
            inboxMessage.Error);

        var outboxMessage =
    await assertionDbContext.OutboxMessages
        .AsNoTracking()
        .SingleAsync(x =>
            x.Type ==
            typeof(InventoryReservedIntegrationEvent).FullName);

        Assert.Null(outboxMessage.ProcessedOnUtc);
        Assert.Null(outboxMessage.Error);

        var reservedintegrationEvent =
    JsonSerializer.Deserialize<InventoryReservedIntegrationEvent>(
        outboxMessage.Content);

        Assert.NotNull(reservedintegrationEvent);

        Assert.Equal(
            orderId,
            reservedintegrationEvent.OrderId);

        Assert.NotEqual(
            Guid.Empty,
            integrationEvent.MessageId);

        Assert.NotEqual(
            default,
            reservedintegrationEvent.OccurredOnUtc);
    }
}
