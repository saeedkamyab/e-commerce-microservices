using Catalog.Application.Products.Commands.ChangeProductPrice;
using MediatR;

namespace Catalog.API.Endpoints.Products;

public static class ChangeProductPriceEndpoint
{
    public static IEndpointRouteBuilder MapChangeProductPriceEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/products/{id:guid}/price",
            async (
                Guid id,
                ChangeProductPriceRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new ChangeProductPriceCommand(
                        id,
                        request.Price,
                        request.Currency),
                    cancellationToken);

                return Results.NoContent();
            });

        return endpoints;
    }
}

public sealed record ChangeProductPriceRequest(
    decimal Price,
    string Currency);
