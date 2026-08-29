using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Persistence;
using Payment.Application.Payments.Commands.CreatePayment;
using Payment.Infrastructure.Persistence.Repositories;
using Payment.IntegrationTests.Infrastructure;

namespace Payment.IntegrationTests.Applications.Payments;

[Collection(PaymentDatabaseCollection.Name)]
public sealed class CreatePaymentTests
{
    private readonly PaymentDatabaseFixture _fixture;

    public CreatePaymentTests(
        PaymentDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Payment()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        IPaymentRepository repository =
            new PaymentRepository(dbContext);

        IUnitOfWork unitOfWork =
            dbContext;

        var handler =
            new CreatePaymentCommandHandler(
                repository,
                unitOfWork);

        var orderId = Guid.NewGuid();

        var command =
            new CreatePaymentCommand(
                orderId,
                2500m);

        var paymentId =
            await handler.Handle(
                command,
                CancellationToken.None);

        await using var assertionContext =
            _fixture.CreateDbContext();

        var payment =
            await assertionContext.Payments
                .AsNoTracking()
                .SingleAsync(x => x.Id == paymentId);

        Assert.Equal(orderId, payment.OrderId);
        Assert.Equal(2500m, payment.Amount);
        Assert.Equal(
            Domain.Enums.PaymentStatus.Pending,
            payment.Status);
    }
}
