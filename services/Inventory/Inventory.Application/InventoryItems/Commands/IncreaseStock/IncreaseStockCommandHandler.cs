using Inventory.Application.Abstractions.Persistence;
using MediatR;

namespace Inventory.Application.InventoryItems.Commands.IncreaseStock;

public sealed class IncreaseStockCommandHandler
    : IRequestHandler<IncreaseStockCommand>
{
    private readonly IInventoryItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public IncreaseStockCommandHandler(
        IInventoryItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        IncreaseStockCommand request,
        CancellationToken cancellationToken)
    {
        var inventoryItem =
            await _repository.GetByProductIdAsync(
                request.ProductId,
                cancellationToken);

        if (inventoryItem is null)
        {
            throw new InvalidOperationException(
                "Inventory item was not found.");
        }

        inventoryItem.IncreaseStock(request.Quantity);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
