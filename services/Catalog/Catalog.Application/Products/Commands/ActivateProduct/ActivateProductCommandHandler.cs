using Catalog.Application.Abstractions.Persistence;
using MediatR;

namespace Catalog.Application.Products.Commands.ActivateProduct;

public sealed class ActivateProductCommandHandler
    : IRequestHandler<ActivateProductCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ActivateProductCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ActivateProductCommand request,
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

        product.Activate();

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
