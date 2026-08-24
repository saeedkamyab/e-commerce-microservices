using Inventory.Domain.Entities;

namespace Inventory.Application.Abstractions.Persistence;

public interface IInventoryItemRepository
{
    Task<InventoryItem?> GetByProductIdAsync(
        Guid productId,
        CancellationToken cancellationToken);

    Task AddAsync(
        InventoryItem inventoryItem,
        CancellationToken cancellationToken);
}
