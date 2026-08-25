using MediatR;

namespace Inventory.Application.InventoryItems.Commands.CreateInventoryItem;

public sealed record CreateInventoryItemCommand(
    Guid ProductId
) : IRequest;
