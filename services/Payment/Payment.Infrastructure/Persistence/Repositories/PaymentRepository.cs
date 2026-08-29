using Microsoft.EntityFrameworkCore;
using Payment.Application.Abstractions.Persistence;

namespace Payment.Infrastructure.Persistence.Repositories;

internal sealed class PaymentRepository
    : IPaymentRepository
{
    private readonly PaymentDbContext _dbContext;

    public PaymentRepository(
        PaymentDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Domain.Payments.Payment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Payments
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    public async Task AddAsync(
        Domain.Payments.Payment payment,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Payments.AddAsync(
            payment,
            cancellationToken);
    }
}
