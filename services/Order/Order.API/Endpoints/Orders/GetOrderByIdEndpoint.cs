using MediatR;
using Order.Application.Orders.Queries.GetOrderById;

namespace Order.API.Endpoints.Orders;

public static class GetOrderByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetOrderByIdEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/orders/{id:guid}",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetOrderByIdQuery(id),
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            });

        return endpoints;
    }
}
