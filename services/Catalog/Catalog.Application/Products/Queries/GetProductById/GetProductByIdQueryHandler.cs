using Catalog.Application.Abstractions.Persistence.Queries;
using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById;

public sealed class GetProductByIdQueryHandler
    : IRequestHandler<GetProductByIdQuery, ProductDetailsResponse?>
{
    private readonly IProductReadService _productReadService;

    public GetProductByIdQueryHandler(
        IProductReadService productReadService)
    {
        _productReadService = productReadService;
    }

    public Task<ProductDetailsResponse?> Handle(
        GetProductByIdQuery request,
        CancellationToken cancellationToken)
    {
        return _productReadService.GetByIdAsync(
            request.ProductId,
            cancellationToken);
    }
}
