using Catalog.Application.Products.Commands.ActivateProduct;
using MediatR;

namespace Catalog.API.Endpoints.Products;

public static class ActivateProductEndpoint
{
    public static IEndpointRouteBuilder MapActivateProductEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/products/{id:guid}/activate",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new ActivateProductCommand(id),
                    cancellationToken);

                return Results.NoContent();
            });

        return endpoints;
    }
}
