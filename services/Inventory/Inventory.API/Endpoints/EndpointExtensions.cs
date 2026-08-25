using Inventory.API.Endpoints.InventoryItems;

namespace Inventory.API.Endpoints;

public static class EndpointExtensions
{
    public static IEndpointRouteBuilder MapInventoryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapIncreaseStockEndpoint();


        return endpoints;
    }
}
