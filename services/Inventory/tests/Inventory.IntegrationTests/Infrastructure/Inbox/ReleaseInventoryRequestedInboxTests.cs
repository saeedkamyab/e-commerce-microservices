using Inventory.Application.Abstractions.Messaging;
using Inventory.Contracts.IntegrationEvents;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence.Inbox;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Order.Contracts.IntegrationEvents;
using System.Text.Json;

namespace Inventory.IntegrationTests.Infrastructure.Inbox;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class ReleaseInventoryRequestedInboxTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public ReleaseInventoryRequestedInboxTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_Inventory_Is_Released_Should_Decrease_ReservedQuantity_And_Create_Outbox_Message()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.InboxMessages
            .ExecuteDeleteAsync();

        await dbContext.OutboxMessages
            .ExecuteDeleteAsync();

        await dbContext.InventoryItems
            .ExecuteDeleteAsync();

        var productId = Guid.NewGuid();
        var orderId = Guid.NewGuid();

        var inventoryItem =
            Inventory.Domain.Entities.InventoryItem.Create(
                productId);

        inventoryItem.IncreaseStock(10);

        inventoryItem.Reserve(2);

        dbContext.InventoryItems.Add(
            inventoryItem);

        await dbContext.SaveChangesAsync();

        Assert.Equal(
            2,
            inventoryItem.ReservedQuantity);

        var integrationEvent =
            new ReleaseInventoryRequestedIntegrationEvent(
                Guid.NewGuid(),
                orderId,
                new[]
                {
                    new ReleaseInventoryItem(
                        productId,
                        2)
                },
                DateTime.UtcNow);

        var repository =
            new InventoryItemRepository(
                dbContext);

        var handler =
            new ReleaseInventoryRequestedIntegrationEventHandler(
                repository,
                dbContext,
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
                typeof(
                    ReleaseInventoryRequestedIntegrationEvent)
                    .FullName!,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(processed);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedInventoryItem =
            await assertionContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(x =>
                    x.ProductId == productId);

        Assert.Equal(
            0,
            persistedInventoryItem.ReservedQuantity);

        Assert.Equal(
            10,
            persistedInventoryItem.AvailableQuantity);

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
                        InventoryReleasedIntegrationEvent)
                    .FullName);

        Assert.Null(
            outboxMessage.ProcessedOnUtc);

        Assert.Null(
            outboxMessage.Error);

        var releasedEvent =
            JsonSerializer.Deserialize<
                InventoryReleasedIntegrationEvent>(
                outboxMessage.Content);

        Assert.NotNull(releasedEvent);

        Assert.Equal(
            orderId,
            releasedEvent.OrderId);
    }
}
