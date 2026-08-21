using Catalog.Application.Products.Commands.DeactivateProduct;
using MediatR;

namespace Catalog.API.Endpoints.Products;

public static class DeactivateProductEndpoint
{
    public static IEndpointRouteBuilder MapDeactivateProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/products/{id:guid}/deactivate",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new DeactivateProductCommand(id),
                    cancellationToken);

                return Results.NoContent();
            });

        return endpoints;
    }
}
