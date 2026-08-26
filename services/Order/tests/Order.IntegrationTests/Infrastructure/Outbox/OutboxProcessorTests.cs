using Microsoft.EntityFrameworkCore;
using Moq;
using Order.Application.Abstractions.Messaging;
using Order.Infrastructure.Persistence.Models;
using Order.Infrastructure.Persistence.Outbox;

namespace Order.IntegrationTests.Infrastructure.Outbox;

[Collection(OrderDatabaseCollection.Name)]
public sealed class OutboxProcessorTests
{
    private readonly OrderDatabaseFixture _fixture;

    public OutboxProcessorTests(
        OrderDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_Should_Publish_And_Mark_Message_As_Processed()
    {
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

        await processor.ProcessAsync(
            CancellationToken.None);

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
                .SingleAsync(x => x.Id == message.Id);

        Assert.NotNull(persistedMessage.ProcessedOnUtc);
        Assert.Null(persistedMessage.Error);
    }
}
