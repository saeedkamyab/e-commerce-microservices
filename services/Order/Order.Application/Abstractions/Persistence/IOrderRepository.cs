namespace Order.Application.Abstractions.Persistence;

public interface IOrderRepository
{
    Task<Order.Domain.Entities.Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

    Task<Order.Domain.Entities.Order?> GetByIdForUserAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken);

    Task AddAsync(
        Domain.Entities.Order order,
        CancellationToken cancellationToken);
}
