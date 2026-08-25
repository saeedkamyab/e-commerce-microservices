using MediatR;

namespace Inventory.Application.InventoryItems.Commands.IncreaseStock;

public sealed record IncreaseStockCommand(
    Guid ProductId,
    int Quantity
) : IRequest;