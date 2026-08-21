namespace Catalog.Application.Products.Queries.GetProductById;

public sealed record ProductDetailsResponse(
    Guid Id,
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    string Currency,
    string Status,
    IReadOnlyCollection<ProductSpecificationResponse> Specifications);

public sealed record ProductSpecificationResponse(
    Guid AttributeDefinitionId,
    string Value);
