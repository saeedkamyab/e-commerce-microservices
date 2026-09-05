using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Persistence;

namespace Order.Infrastructure.Persistence.Repositories;

internal sealed class OrderRepository
    : IOrderRepository
{
    private readonly OrderDbContext _dbContext;

    public OrderRepository(
        OrderDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<Order.Domain.Entities.Order?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }
    public Task<Order.Domain.Entities.Order?> GetByIdForUserAsync(
    Guid id,
    Guid userId,
    CancellationToken cancellationToken)
    {
        return _dbContext.Orders
            .Include(x => x.Items)
            .FirstOrDefaultAsync(
                x => x.Id == id && x.UserId == userId,
                cancellationToken);
    }
    public async Task AddAsync(
        Order.Domain.Entities.Order order,
        CancellationToken cancellationToken)
    {
        await _dbContext.Orders.AddAsync(
            order,
            cancellationToken);
    }
}
