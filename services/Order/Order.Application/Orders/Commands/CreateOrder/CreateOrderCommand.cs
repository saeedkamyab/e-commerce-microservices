using MediatR;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    Guid UserId,
    IReadOnlyCollection<CreateOrderItemInput> Items
) : IRequest<Guid>;

public sealed record CreateOrderItemInput(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice);
