using Inventory.Application.InventoryItems.Commands.IncreaseStock;
using MediatR;

namespace Inventory.API.Endpoints.InventoryItems;

public static class IncreaseStockEndpoint
{
    public static IEndpointRouteBuilder MapIncreaseStockEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPut(
            "/api/inventory/{productId:guid}/increase",
            async (
                Guid productId,
                IncreaseStockRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                await sender.Send(
                    new IncreaseStockCommand(
                        productId,
                        request.Quantity),
                    cancellationToken);

                return Results.NoContent();
            });

        return endpoints;
    }
}

public sealed record IncreaseStockRequest(
    int Quantity);
