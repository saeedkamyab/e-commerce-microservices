namespace Payment.Application.Abstractions.Persistence;

public interface IPaymentRepository
{
    Task<Domain.Payments.Payment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Domain.Payments.Payment payment,
        CancellationToken cancellationToken = default);
}
