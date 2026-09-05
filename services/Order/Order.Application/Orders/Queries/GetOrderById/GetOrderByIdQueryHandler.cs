using MediatR;
using Order.Application.Abstractions.Authentication;
using Order.Application.Abstractions.Persistence;

namespace Order.Application.Orders.Queries.GetOrderById;

public sealed class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDetailsResponse?>
{
    private readonly IOrderRepository _orderRepository;
    private readonly ICurrentUser _currentUser;

    public GetOrderByIdQueryHandler(
        IOrderRepository orderRepository,
        ICurrentUser currentUser)
    {
        _orderRepository = orderRepository;
        _currentUser = currentUser;
    }

    public async Task<OrderDetailsResponse?> Handle(
        GetOrderByIdQuery request,
        CancellationToken cancellationToken)
    {
        var order =
            await _orderRepository.GetByIdForUserAsync(
                request.OrderId,
                 _currentUser.UserId,
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
