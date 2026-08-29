namespace Order.Application.Orders.Queries.GetOrderById;

public sealed record OrderDetailsResponse(
    Guid Id,
    Guid UserId,
    string Status,
    decimal TotalAmount,
    IReadOnlyCollection<OrderItemResponse> Items);

public sealed record OrderItemResponse(
    Guid ProductId,
    int Quantity,
    decimal UnitPrice,
    decimal Total);
