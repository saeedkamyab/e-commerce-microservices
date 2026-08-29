using MediatR;
using Order.Application.Abstractions.Persistence;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDetailsResponse?>
{
    private readonly IOrderRepository _orderRepository;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository)
    {
        _orderRepository = orderRepository;
    }

    public async Task<OrderDetailsResponse?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order =
            await _orderRepository.GetByIdAsync(
                request.OrderId,
                cancellationToken);

        if (order is null)
            return null;

        return new OrderDetailsResponse(
            order.Id,
            order.UserId,
            order.Status.ToString(),
            order.TotalAmount,
            order.Items
                .Select(x =>
                    new OrderItemResponse(
                        x.ProductId,
                        x.Quantity,
                        x.UnitPrice,
                        x.Total))
                .ToArray());
    }
}
