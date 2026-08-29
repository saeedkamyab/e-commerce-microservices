using Inventory.Application.Abstractions.Messaging;
using Inventory.Infrastructure.Messaging;
using Inventory.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
namespace Inventory.IntegrationTests.Infrastructure.Inbox;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class InboxTransactionRollbackTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public InboxTransactionRollbackTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_Handler_Fails_Should_Rollback_Transaction()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var handler =
            new FailingIntegrationEventHandler();

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var processor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        var messageId = Guid.NewGuid();

        // Act
        var action = () =>
            processor.ProcessAsync(
                messageId,
                "TestIntegrationEvent",
                """{"value":"test"}""",
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            action);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var inboxExists =
            await assertionDbContext.InboxMessages
                .AnyAsync(
                    x => x.MessageId == messageId);

        Assert.False(inboxExists);
    }

    [Fact]
    public async Task ProcessAsync_When_Handler_Fails_After_Database_Change_Should_Rollback_All_Changes()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var productId = Guid.NewGuid();
        var messageId = Guid.NewGuid();

        var handler =
            new FailingAfterDatabaseChangeHandler(
                dbContext,
                productId);

        var dispatcher =
            new IntegrationEventDispatcher(
                [handler]);

        var processor =
            new InboxProcessor(
                dbContext,
                dispatcher);

        // Act
        var action = () =>
            processor.ProcessAsync(
                messageId,
                "TestIntegrationEvent",
                """{"value":"test"}""",
                CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            action);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var inventoryExists =
            await assertionDbContext.InventoryItems
                .AnyAsync(
                    x => x.ProductId == productId);

        var inboxExists =
            await assertionDbContext.InboxMessages
                .AnyAsync(
                    x => x.MessageId == messageId);

        Assert.False(inventoryExists);
        Assert.False(inboxExists);
    }

    private sealed class FailingIntegrationEventHandler
        : IIntegrationEventHandler
    {
        public string EventType =>
            "TestIntegrationEvent";

        public Task HandleAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException(
                "Simulated business failure.");
        }
    }

    private sealed class FailingAfterDatabaseChangeHandler
        : IIntegrationEventHandler
    {
        private readonly Inventory.Infrastructure.Persistence.InventoryDbContext
            _dbContext;

        private readonly Guid _productId;

        public FailingAfterDatabaseChangeHandler(
            Inventory.Infrastructure.Persistence.InventoryDbContext dbContext,
            Guid productId)
        {
            _dbContext = dbContext;
            _productId = productId;
        }

        public string EventType =>
            "TestIntegrationEvent";

        public async Task HandleAsync(
            Guid messageId,
            string type,
            string content,
            CancellationToken cancellationToken)
        {
            var inventoryItem =
                Inventory.Domain.Entities.InventoryItem.Create(
                    _productId);

            await _dbContext.InventoryItems.AddAsync(
                inventoryItem,
                cancellationToken);

            await _dbContext.SaveChangesAsync(
                cancellationToken);

            throw new InvalidOperationException(
                "Simulated failure after database change.");
        }
    }
}