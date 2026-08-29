using Catalog.Contracts.IntegrationEvents;
using Inventory.Application.Abstractions.Messaging;
using Inventory.Application.Abstractions.Persistence;
using Inventory.Application.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence.Inbox;
using Inventory.Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Inventory.IntegrationTests.Infrastructure.Inbox;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class ProductActivatedInboxTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public ProductActivatedInboxTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_With_Same_Message_Twice_Should_Create_Inventory_Item_Only_Once()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var commandHandler =
            new CreateInventoryItemCommandHandler(
                repository,
                unitOfWork);

        var integrationEventHandler =
            new TestProductActivatedIntegrationEventHandler(
                commandHandler);

        var dispatcher =
            new IntegrationEventDispatcher(
                [integrationEventHandler]);

        var inboxProcessor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var integrationEvent =
            new ProductActivatedIntegrationEvent(
                messageId,
                productId,
                DateTime.UtcNow);

        var type =
            typeof(ProductActivatedIntegrationEvent).FullName!;

        var content =
            JsonSerializer.Serialize(integrationEvent);

        // Act - first delivery
        var firstResult =
            await inboxProcessor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Act - duplicate delivery
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

        var inventoryItemsCount =
            await assertionDbContext.InventoryItems
                .CountAsync(
                    x => x.ProductId == productId);

        Assert.Equal(
            1,
            inventoryItemsCount);

        var inboxMessagesCount =
            await assertionDbContext.InboxMessages
                .CountAsync(
                    x => x.MessageId == messageId);

        Assert.Equal(
            1,
            inboxMessagesCount);

        var inboxMessage =
            await assertionDbContext.InboxMessages
                .AsNoTracking()
                .SingleAsync(
                    x => x.MessageId == messageId);

        Assert.NotNull(
            inboxMessage.ProcessedOnUtc);

        Assert.Null(
            inboxMessage.Error);
    }

    private sealed class TestProductActivatedIntegrationEventHandler
        : IIntegrationEventHandler
    {
        private readonly CreateInventoryItemCommandHandler _handler;

        public TestProductActivatedIntegrationEventHandler(
            CreateInventoryItemCommandHandler handler)
        {
            _handler = handler;
        }

        public string EventType =>
            typeof(ProductActivatedIntegrationEvent).FullName!;

        public async Task HandleAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            var integrationEvent =
                JsonSerializer.Deserialize<ProductActivatedIntegrationEvent>(
                    content)
                ?? throw new InvalidOperationException(
                    "Could not deserialize ProductActivatedIntegrationEvent.");

            await _handler.Handle(
                new CreateInventoryItemCommand(
                    integrationEvent.ProductId),
                cancellationToken);
        }
    }
}
