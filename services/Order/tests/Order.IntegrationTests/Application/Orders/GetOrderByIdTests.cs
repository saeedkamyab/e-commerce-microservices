using Microsoft.EntityFrameworkCore;
using Order.Application.Abstractions.Authentication;
using Order.Application.Orders.Queries.GetOrderById;
using Order.Domain.Entities;
using Order.Infrastructure.Persistence.Repositories;
using Order.IntegrationTests.Infrastructure;

namespace Order.IntegrationTests.Application.Orders;

[Collection(OrderDatabaseCollection.Name)]
public sealed class GetOrderByIdTests
{
    private readonly OrderDatabaseFixture _fixture;

    public GetOrderByIdTests(
        OrderDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Return_Order_When_Order_Belongs_To_Current_User()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.OrderItems.ExecuteDeleteAsync();
        await dbContext.Orders.ExecuteDeleteAsync();

        var currentUserId = Guid.NewGuid();

        var order =
            CreateOrder(
                currentUserId);

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        var repository =
            new OrderRepository(dbContext);

        var currentUser =
            new FakeCurrentUser(currentUserId);

        var handler =
            new GetOrderByIdQueryHandler(
                repository,
                currentUser);

        var query =
            new GetOrderByIdQuery(
                order.Id);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.NotNull(result);

        Assert.Equal(
            order.Id,
            result.Id);

        Assert.Equal(
            currentUserId,
            result.UserId);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Order_Belongs_To_Another_User()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.OrderItems.ExecuteDeleteAsync();
        await dbContext.Orders.ExecuteDeleteAsync();

        var ownerUserId = Guid.NewGuid();
        var anotherUserId = Guid.NewGuid();

        var order =
            CreateOrder(
                ownerUserId);

        dbContext.Orders.Add(order);

        await dbContext.SaveChangesAsync();

        var repository =
            new OrderRepository(dbContext);

        var currentUser =
            new FakeCurrentUser(
                anotherUserId);

        var handler =
            new GetOrderByIdQueryHandler(
                repository,
                currentUser);

        var query =
            new GetOrderByIdQuery(
                order.Id);

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Order_Does_Not_Exist()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        await dbContext.OrderItems.ExecuteDeleteAsync();
        await dbContext.Orders.ExecuteDeleteAsync();

        var currentUserId =
            Guid.NewGuid();

        var repository =
            new OrderRepository(dbContext);

        var currentUser =
            new FakeCurrentUser(
                currentUserId);

        var handler =
            new GetOrderByIdQueryHandler(
                repository,
                currentUser);

        var query =
            new GetOrderByIdQuery(
                Guid.NewGuid());

        var result =
            await handler.Handle(
                query,
                CancellationToken.None);

        Assert.Null(result);
    }

    private static Order.Domain.Entities.Order CreateOrder(
        Guid userId)
    {
        var productId =
            Guid.NewGuid();

        var item =
            OrderItem.Create(
                productId,
                2,
                100m);

        var order =
            Order.Domain.Entities.Order.Create(
                userId,
                new[]
                {
                    item
                });

        return order;
    }

    private sealed class FakeCurrentUser
        : ICurrentUser
    {
        public FakeCurrentUser(
            Guid userId)
        {
            UserId = userId;
        }

        public Guid UserId { get; }
    }
}
