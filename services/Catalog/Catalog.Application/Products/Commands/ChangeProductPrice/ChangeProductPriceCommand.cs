using MediatR;

namespace Catalog.Application.Products.Commands.ChangeProductPrice;

public sealed record ChangeProductPriceCommand(
    Guid ProductId,
    decimal Price,
    string Currency
) : IRequest;
