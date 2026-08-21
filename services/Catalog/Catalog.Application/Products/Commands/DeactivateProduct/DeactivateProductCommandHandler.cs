using Catalog.Application.Abstractions.Persistence;
using MediatR;

namespace Catalog.Application.Products.Commands.DeactivateProduct;

public sealed class DeactivateProductCommandHandler
    : IRequestHandler<DeactivateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        DeactivateProductCommand request,
        CancellationToken cancellationToken)
    {
        var product = await _productRepository.GetByIdAsync(
            request.ProductId,
            cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException(
                "Product was not found.");
        }

        product.Deactivate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
