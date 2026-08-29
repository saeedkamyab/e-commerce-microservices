using MediatR;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed record GetOrderByIdQuery(
    Guid OrderId
) : IRequest<OrderDetailsResponse?>;
