using Microsoft.EntityFrameworkCore;
using Payment.Contracts.IntegrationEvents;
using Payment.Infrastructure.Persistence.Outbox;
using Payment.IntegrationTests.Fakes;
using System.Text.Json;

namespace Payment.IntegrationTests.Infrastructure.Outbox;

[Collection(PaymentDatabaseCollection.Name)]
public sealed class OutboxProcessorTests
{
    private readonly PaymentDatabaseFixture _fixture;

    public OutboxProcessorTests(
        PaymentDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_Publish_Succeeds_Should_Mark_Message_As_Processed()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.OutboxMessages
    .ExecuteDeleteAsync();

        var integrationEvent =
            new PaymentSucceededIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                1200m,
                DateTime.UtcNow);

        var message =
            new OutboxMessage
            {
                Id = integrationEvent.MessageId,
                Type = typeof(
                    PaymentSucceededIntegrationEvent).FullName!,
                Content = JsonSerializer.Serialize(
                    integrationEvent),
                OccurredOnUtc =
                    integrationEvent.OccurredOnUtc
            };

        dbContext.OutboxMessages.Add(message);

        await dbContext.SaveChangesAsync();

        var publisher =
            new FakeIntegrationEventPublisher();

        var processor =
            new OutboxProcessor(
                dbContext,
                publisher);

        // Act
        await processor.ProcessAsync(
            CancellationToken.None);

        // Assert
        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == message.Id);

        Assert.NotNull(
            persistedMessage.ProcessedOnUtc);

        Assert.Null(
            persistedMessage.Error);

        Assert.Equal(
            1,
            publisher.PublishCount);
    }

    [Fact]
    public async Task ProcessAsync_When_Publish_Fails_Should_Keep_Message_Unprocessed_And_Store_Error()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.OutboxMessages
    .ExecuteDeleteAsync();

        var integrationEvent =
            new PaymentFailedIntegrationEvent(
                Guid.NewGuid(),
                Guid.NewGuid(),
                Guid.NewGuid(),
                "Payment was declined.",
                DateTime.UtcNow);

        var message =
            new OutboxMessage
            {
                Id = integrationEvent.MessageId,
                Type = typeof(
                    PaymentFailedIntegrationEvent).FullName!,
                Content = JsonSerializer.Serialize(
                    integrationEvent),
                OccurredOnUtc =
                    integrationEvent.OccurredOnUtc
            };

        dbContext.OutboxMessages.Add(message);

        await dbContext.SaveChangesAsync();

        var publisher =
            new FakeIntegrationEventPublisher(
                shouldFail: true);

        var processor =
            new OutboxProcessor(
                dbContext,
                publisher);

        // Act
        await processor.ProcessAsync(
            CancellationToken.None);

        // Assert
        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Id == message.Id);

        Assert.Null(
            persistedMessage.ProcessedOnUtc);

        Assert.NotNull(
            persistedMessage.Error);

        Assert.Contains(
            "Simulated publish failure",
            persistedMessage.Error);

        Assert.Equal(
            1,
            publisher.PublishCount);
    }
}
