using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Products.Commands.ChangeProductPrice;

public sealed class ChangeProductPriceCommandHandler
    : IRequestHandler<ChangeProductPriceCommand>
{
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ChangeProductPriceCommandHandler(
        IProductRepository productRepository,
        IUnitOfWork unitOfWork)
    {
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task Handle(
        ChangeProductPriceCommand request,
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

        var newPrice = Price.Create(
            request.Price,
            request.Currency);

        product.ChangePrice(newPrice);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);
    }
}
