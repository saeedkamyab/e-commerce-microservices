using Inventory.Application.Abstractions.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Inventory.Application.InventoryItems.Commands.ReserveStock;

public sealed class ReserveStockCommandHandler
    : IRequestHandler<ReserveStockCommand>
{
    private readonly IInventoryItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public ReserveStockCommandHandler(
        IInventoryItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
     ReserveStockCommand request,
     CancellationToken cancellationToken)
    {
        const int maxAttempts = 3;

        for (var attempt = 1; attempt <= maxAttempts; attempt++)
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

            try
            {
                inventoryItem.Reserve(request.Quantity);

                await _unitOfWork.SaveChangesAsync(
                    cancellationToken);

                return;
            }
            catch (DbUpdateConcurrencyException)
                when (attempt < maxAttempts)
            {
                await _repository.ReloadByProductIdAsync(
                    request.ProductId,
                    cancellationToken);
            }
        }

        throw new InvalidOperationException(
            "Could not reserve inventory because of repeated concurrent updates.");
    }
}