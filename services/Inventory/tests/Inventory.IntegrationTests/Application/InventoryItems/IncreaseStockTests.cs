using Inventory.Application.Abstractions.Persistence;
using Inventory.Application.InventoryItems.Commands.IncreaseStock;
using Inventory.Domain.Entities;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests.Application.InventoryItems;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class IncreaseStockTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public IncreaseStockTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Increase_Stock_And_Persist_It()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var productId = Guid.NewGuid();

        var inventoryItem =
            InventoryItem.Create(productId);

        dbContext.InventoryItems.Add(inventoryItem);

        await dbContext.SaveChangesAsync();

        var repository =
            new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new IncreaseStockCommandHandler(
                repository,
                unitOfWork);

        // Act
        await handler.Handle(
            new IncreaseStockCommand(
                productId,
                10),
            CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedItem =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(
                    x => x.ProductId == productId);

        Assert.Equal(10, persistedItem.Quantity);
        Assert.Equal(0, persistedItem.ReservedQuantity);
        Assert.Equal(10, persistedItem.AvailableQuantity);
    }
    [Fact]
    public async Task Handle_When_Inventory_Item_Does_Not_Exist_Should_Throw()
    {
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new IncreaseStockCommandHandler(
                repository,
                unitOfWork);

        var action = () =>
            handler.Handle(
                new IncreaseStockCommand(
                    Guid.NewGuid(),
                    10),
                CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(
            action);
    }
}
