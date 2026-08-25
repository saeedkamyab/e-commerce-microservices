using Inventory.Application.Abstractions.Persistence;
using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Infrastructure.Persistence.Repositories;

internal sealed class InventoryItemRepository
    : IInventoryItemRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryItemRepository(
        InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<InventoryItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        return _dbContext.InventoryItems
            .FirstOrDefaultAsync(
                x => x.ProductId == productId,
                cancellationToken);
    }


    public async Task<InventoryItem?> ReloadByProductIdAsync(
    Guid productId,
    CancellationToken cancellationToken)
    {
        var trackedEntry = _dbContext.ChangeTracker
            .Entries<InventoryItem>()
            .FirstOrDefault(x => x.Entity.ProductId == productId);

        if (trackedEntry is not null)
        {
            await trackedEntry.ReloadAsync(cancellationToken);
            return trackedEntry.Entity;
        }

        return await _dbContext.InventoryItems
            .FirstOrDefaultAsync(
                x => x.ProductId == productId,
                cancellationToken);
    }

    public async Task AddAsync(
        InventoryItem inventoryItem,
        CancellationToken cancellationToken)
    {
        await _dbContext.InventoryItems.AddAsync(
            inventoryItem,
            cancellationToken);
    }
}
