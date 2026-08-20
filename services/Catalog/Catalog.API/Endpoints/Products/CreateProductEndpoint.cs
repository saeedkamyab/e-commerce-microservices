using Catalog.Application.Products.Commands.CreateProduct;
using MediatR;

namespace Catalog.API.Endpoints.Products;

public static class CreateProductEndpoint
{
    public static IEndpointRouteBuilder MapCreateProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/products",
            async (
                CreateProductRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateProductCommand(
                    request.Name,
                    request.Description,
                    request.CategoryId,
                    request.Price,
                    request.Currency,
                    request.Specifications
                        .Select(x =>
                            new ProductSpecificationInput(
                                x.AttributeDefinitionId,
                                x.Value))
                        .ToArray());

                var productId = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/products/{productId}",
                    new { Id = productId });
            });

        return endpoints;
    }
}

public sealed record CreateProductRequest(
    string Name,
    string? Description,
    Guid CategoryId,
    decimal Price,
    string Currency,
    IReadOnlyCollection<ProductSpecificationRequest> Specifications);

public sealed record ProductSpecificationRequest(
    Guid AttributeDefinitionId,
    string Value);
