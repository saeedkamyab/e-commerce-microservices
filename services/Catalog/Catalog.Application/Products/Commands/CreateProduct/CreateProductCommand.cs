using MediatR;

namespace Catalog.Application.Products.Commands.CreateProduct;

public sealed record CreateProductCommand(
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    string Currency,
IReadOnlyCollection<ProductSpecificationInput> Specifications
) : IRequest<Guid>;

public sealed record ProductSpecificationInput(
    Guid AttributeDefinitionId,
    string Value);