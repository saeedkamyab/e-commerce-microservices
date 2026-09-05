using MediatR;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed record CreateOrderCommand(
    IReadOnlyCollection<CreateOrderItemInput> Items
) : IRequest<Guid>;

public sealed record CreateOrderItemInput(
    Guid ProductId,
    int Quantity);
