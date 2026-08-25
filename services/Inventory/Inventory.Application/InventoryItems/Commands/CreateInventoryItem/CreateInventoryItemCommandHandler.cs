using Inventory.Application.Abstractions.Persistence;
using Inventory.Domain.Entities;
using MediatR;

namespace Inventory.Application.InventoryItems.Commands.CreateInventoryItem;

public sealed class CreateInventoryItemCommandHandler
    : IRequestHandler<CreateInventoryItemCommand>
{
    private readonly IInventoryItemRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateInventoryItemCommandHandler(
        IInventoryItemRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        CreateInventoryItemCommand request,
        CancellationToken cancellationToken)
    {
        var existingItem =
            await _repository.GetByProductIdAsync(
                request.ProductId,
                cancellationToken);

        if (existingItem is not null)
            return;

        var inventoryItem =
            InventoryItem.Create(request.ProductId);

        await _repository.AddAsync(
            inventoryItem,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
