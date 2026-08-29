using Inventory.Application.Abstractions.Messaging;
using Inventory.Infrastructure.Persistence.Models;
using Inventory.Infrastructure.Persistence.Outbox;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Inventory.IntegrationTests.Infrastructure.Outbox;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class OutboxProcessorTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public OutboxProcessorTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_Should_Publish_And_Mark_Message_As_Processed()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestIntegrationEvent",
            Content = """{"value":"test"}""",
            OccurredOnUtc = DateTime.UtcNow
        };

        dbContext.OutboxMessages.Add(message);

        await dbContext.SaveChangesAsync();

        var publisher =
            new Mock<IIntegrationEventPublisher>();

        var processor =
            new OutboxProcessor(
                dbContext,
                publisher.Object);

        // Act
        await processor.ProcessAsync(
            CancellationToken.None);

        // Assert
        publisher.Verify(
            x => x.PublishAsync(
                message.Id,
                message.Type,
                message.Content,
                It.IsAny<CancellationToken>()),
            Times.Once);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedMessage =
            await assertionDbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == message.Id);

        Assert.NotNull(
            persistedMessage.ProcessedOnUtc);

        Assert.Null(
            persistedMessage.Error);
    }

    [Fact]
    public async Task ProcessAsync_When_Publisher_Fails_Should_Store_Error_And_Not_Mark_As_Processed()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var message = new OutboxMessage
        {
            Id = Guid.NewGuid(),
            Type = "TestIntegrationEvent",
            Content = """{"value":"test"}""",
            OccurredOnUtc = DateTime.UtcNow
        };

        dbContext.OutboxMessages.Add(message);

        await dbContext.SaveChangesAsync();

        var publisher =
            new Mock<IIntegrationEventPublisher>();

        publisher
            .Setup(x => x.PublishAsync(
                message.Id,
                message.Type,
                message.Content,
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(
                new InvalidOperationException(
                    "RabbitMQ unavailable"));

        var processor =
            new OutboxProcessor(
                dbContext,
                publisher.Object);

        // Act
        await processor.ProcessAsync(
            CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedMessage =
            await assertionDbContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == message.Id);

        Assert.Null(
            persistedMessage.ProcessedOnUtc);

        Assert.NotNull(
            persistedMessage.Error);

        Assert.Contains(
            "RabbitMQ unavailable",
            persistedMessage.Error);
    }
}
