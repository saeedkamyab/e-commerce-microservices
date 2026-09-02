namespace Order.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,
        AwaitingInventory = 2,
        InventoryReserved = 3,
        AwaitingPayment = 4,
        Paid = 5,
        PaymentFailed=6,
        Cancelled = 7
    }
}
