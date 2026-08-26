using MediatR;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Entities;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var items = request.Items
            .Select(x =>
                OrderItem.Create(
                    x.ProductId,
                    x.Quantity,
                    x.UnitPrice))
            .ToArray();

        var order = Domain.Entities.Order.Create(
            request.UserId,
            items);
        order.StartInventoryReservation();

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return order.Id;
    }
}
