using MediatR;

namespace Catalog.Application.Products.Commands.DeactivateProduct;

public sealed record DeactivateProductCommand(
    Guid ProductId
) : IRequest;
