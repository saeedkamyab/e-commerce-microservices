using MediatR;

namespace Catalog.Application.Products.Queries.GetProductById;

public sealed record GetProductByIdQuery(
    Guid ProductId
) : IRequest<ProductDetailsResponse?>;
