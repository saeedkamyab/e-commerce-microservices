using MediatR;

namespace Inventory.Application.InventoryItems.Commands.ReserveStock;


public sealed record ReserveStockCommand(
    Guid ProductId,
    int Quantity
) : IRequest;
