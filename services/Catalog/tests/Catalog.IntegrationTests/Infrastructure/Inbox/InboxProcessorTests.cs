using Catalog.Application.Abstractions.Messaging;
using Catalog.Infrastructure.Persistence.Inbox;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace Catalog.IntegrationTests.Infrastructure.Inbox;

[Collection(CatalogDatabaseCollection.Name)]
public sealed class InboxProcessorTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public InboxProcessorTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_Should_Process_Message_Only_Once()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var handler =
            new Mock<IIntegrationEventHandler>();

        var processor =
            new InboxProcessor(
                dbContext,
                handler.Object);

        var messageId = Guid.NewGuid();

        const string type =
            "TestIntegrationEvent";

        const string content =
            """{"value":"test"}""";

        // Act - first delivery
        var firstResult =
            await processor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Act - duplicate delivery
        var secondResult =
            await processor.ProcessAsync(
                messageId,
                type,
                content,
                CancellationToken.None);

        // Assert
        Assert.True(firstResult);
        Assert.False(secondResult);

        handler.Verify(
            x => x.HandleAsync(
                messageId,
                type,
                content,
                It.IsAny<CancellationToken>()),
            Times.Once);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

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
}
