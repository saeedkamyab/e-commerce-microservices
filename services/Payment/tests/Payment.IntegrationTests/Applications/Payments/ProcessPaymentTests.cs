using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Payments;
using Payment.Application.Abstractions.Persistence;
using Payment.Application.Payments.Commands.ProcessPayment;
using Payment.Contracts.IntegrationEvents;
using Payment.Domain.Enums;
using Payment.Infrastructure.Persistence.Repositories;
using Payment.IntegrationTests.Fakes;
using Payment.IntegrationTests.Infrastructure;

namespace Payment.IntegrationTests.Applications.Payments;

[Collection(PaymentDatabaseCollection.Name)]
public class ProcessPaymentTests
{
    private readonly PaymentDatabaseFixture _fixture;

    public ProcessPaymentTests(PaymentDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_When_Gateway_Succeeds_Should_Mark_Payment_Succeeded_And_Create_Outbox_Message()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        var payment =
            Payment.Domain.Payments.Payment.Create(
                Guid.NewGuid(),
                1200m);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var repository =
            new PaymentRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var gateway =
            new FakePaymentGateway(
                new PaymentGatewayResult(
                    true,
                    null));

        var handler =
            new ProcessPaymentCommandHandler(
                repository,
                unitOfWork,
                gateway);

        await handler.Handle(
            new ProcessPaymentCommand(payment.Id),
            CancellationToken.None);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedPayment =
            await assertionContext.Payments
                .AsNoTracking()
                .SingleAsync(x => x.Id == payment.Id);

        Assert.Equal(
            PaymentStatus.Succeeded,
            persistedPayment.Status);

        var outboxMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Type ==
                    typeof(PaymentSucceededIntegrationEvent).FullName);

        Assert.Null(outboxMessage.ProcessedOnUtc);
    }

    [Fact]
    public async Task Handle_When_Gateway_Fails_Should_Mark_Payment_Failed_And_Create_Outbox_Message()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        var payment =
            Payment.Domain.Payments.Payment.Create(
                Guid.NewGuid(),
                1200m);

        dbContext.Payments.Add(payment);
        await dbContext.SaveChangesAsync();

        var repository =
            new PaymentRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var gateway =
            new FakePaymentGateway(
                new PaymentGatewayResult(
                    false,
                    "Card was declined."));

        var handler =
            new ProcessPaymentCommandHandler(
                repository,
                unitOfWork,
                gateway);

        await handler.Handle(
            new ProcessPaymentCommand(payment.Id),
            CancellationToken.None);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var persistedPayment =
            await assertionContext.Payments
                .AsNoTracking()
                .SingleAsync(x => x.Id == payment.Id);

        Assert.Equal(
            PaymentStatus.Failed,
            persistedPayment.Status);

        var outboxMessage =
            await assertionContext.OutboxMessages
                .AsNoTracking()
                .SingleAsync(x =>
                    x.Type ==
                    typeof(PaymentFailedIntegrationEvent).FullName);

        Assert.Null(outboxMessage.ProcessedOnUtc);
    }

}
