namespace Payment.Application.Abstractions.Persistence;

public interface IPaymentRepository
{

    Task<IReadOnlyCollection<Domain.Payments.Payment>> GetPendingAsync(
    int batchSize,
    CancellationToken cancellationToken = default);

    Task<Domain.Payments.Payment?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Domain.Payments.Payment payment,
        CancellationToken cancellationToken = default);
}
