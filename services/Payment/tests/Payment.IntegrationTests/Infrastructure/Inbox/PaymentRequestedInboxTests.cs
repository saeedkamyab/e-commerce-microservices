using Microsoft.EntityFrameworkCore;
using Order.Contracts.IntegrationEvents;
using Payment.Application.Abstractions.Persistence;
using Payment.Domain.Enums;
using Payment.Infrastructure.Messaging;
using Payment.Infrastructure.Persistence.Inbox;
using Payment.Infrastructure.Persistence.Repositories;
using System.Text.Json;

namespace Payment.IntegrationTests.Infrastructure.Inbox;

[Collection(PaymentDatabaseCollection.Name)]
public sealed class PaymentRequestedInboxTests
{
    private readonly PaymentDatabaseFixture _fixture;

    public PaymentRequestedInboxTests(
        PaymentDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ProcessAsync_When_Payment_Is_Requested_Should_Create_Payment_And_Process_Inbox()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new PaymentRequestedIntegrationEventHandler(
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
        var orderId = Guid.NewGuid();

        var integrationEvent =
            new PaymentRequestedIntegrationEvent(
                messageId,
                orderId,
                2400m,
                DateTime.UtcNow);

        var type =
            typeof(PaymentRequestedIntegrationEvent)
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

        var payment =
            await assertionDbContext.Payments
                .AsNoTracking()
                .SingleAsync(x =>
                    x.OrderId == orderId);

        Assert.Equal(2400m, payment.Amount);

        Assert.Equal(
            PaymentStatus.Pending,
            payment.Status);

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
    public async Task ProcessAsync_With_Same_Message_Twice_Should_Create_Payment_Only_Once()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new PaymentRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new PaymentRequestedIntegrationEventHandler(
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
        var orderId = Guid.NewGuid();

        var integrationEvent =
            new PaymentRequestedIntegrationEvent(
                messageId,
                orderId,
                2400m,
                DateTime.UtcNow);

        var type =
            typeof(PaymentRequestedIntegrationEvent)
                .FullName!;

        var content =
            JsonSerializer.Serialize(
                integrationEvent);

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

        var paymentCount =
            await assertionDbContext.Payments
                .CountAsync(x =>
                    x.OrderId == orderId);

        Assert.Equal(1, paymentCount);

        var inboxCount =
            await assertionDbContext.InboxMessages
                .CountAsync(x =>
                    x.MessageId == messageId);

        Assert.Equal(1, inboxCount);
    }
}
