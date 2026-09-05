using MediatR;
using Order.Application.Abstractions.Authentication;
using Order.Application.Abstractions.Catalog;
using Order.Application.Abstractions.Persistence;
using Order.Domain.Entities;

namespace Order.Application.Orders.Commands.CreateOrder;

public sealed class CreateOrderCommandHandler
    : IRequestHandler<CreateOrderCommand, Guid>
{
    private readonly IOrderRepository _orderRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUser _currentUser;
    private readonly ICatalogService _catalogService;

    public CreateOrderCommandHandler(
        IOrderRepository orderRepository,
        IUnitOfWork unitOfWork,ICurrentUser currentUser,
            ICatalogService catalogService)
    {
        _orderRepository = orderRepository;
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _catalogService = catalogService;
    }

    public async Task<Guid> Handle(
        CreateOrderCommand request,
        CancellationToken cancellationToken)
    {
        var orderItems =
       new List<OrderItem>();

        foreach (var item in request.Items)
        {
            var product =
               await _catalogService.GetProductAsync(
                   item.ProductId,
                   cancellationToken);

            if (product is null)
            {
                throw new ArgumentException(
                    $"Product '{item.ProductId}' was not found.");
            }

            if (!product.IsActive)
            {
                throw new ArgumentException(
                    $"Product '{item.ProductId}' is not active.");
            }


            var orderItem =
           OrderItem.Create(
               item.ProductId,
               item.Quantity,
               product.Price);

            orderItems.Add(orderItem);

        }

        

        var order = Domain.Entities.Order.Create(
            _currentUser.UserId,
            orderItems);
        order.StartInventoryReservation();

        await _orderRepository.AddAsync(
            order,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return order.Id;
    }
}
