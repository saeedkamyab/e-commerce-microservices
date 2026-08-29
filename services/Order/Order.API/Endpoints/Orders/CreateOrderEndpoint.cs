using MediatR;
using Order.Application.Orders.Commands.CreateOrder;

namespace Order.API.Endpoints.Orders;

public static class CreateOrderEndpoint
{
    public static IEndpointRouteBuilder MapCreateOrderEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/orders",
            async (
                CreateOrderRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateOrderCommand(
                    request.UserId,
                    request.Items
                        .Select(x =>
                            new CreateOrderItemInput(
                                x.ProductId,
                                x.Quantity,
                                x.UnitPrice))
                        .ToArray());

                var orderId = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/orders/{orderId}",
                    new { Id = orderId });
            });

        return endpoints;
    }
}

public sealed record CreateOrderRequest(
    Guid UserId,
    IReadOnlyCollection<CreateOrderItemRequest> Items);

public sealed record CreateOrderItemRequest(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
