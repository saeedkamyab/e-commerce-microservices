using Inventory.Application.Abstractions.Persistence;
using Inventory.Application.InventoryItems.Commands.CreateInventoryItem;
using Inventory.Infrastructure.Persistence.Repositories;
using Inventory.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests.Application.InventoryItems;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class CreateInventoryItemTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public CreateInventoryItemTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Inventory_Item()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new CreateInventoryItemCommandHandler(
                repository,
                unitOfWork);

        var productId = Guid.NewGuid();

        // Act
        await handler.Handle(
            new CreateInventoryItemCommand(productId),
            CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var inventoryItem =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(
                    x => x.ProductId == productId);

        Assert.NotEqual(Guid.Empty, inventoryItem.Id);
        Assert.Equal(productId, inventoryItem.ProductId);

        Assert.Equal(0, inventoryItem.Quantity);
        Assert.Equal(0, inventoryItem.ReservedQuantity);
        Assert.Equal(0, inventoryItem.AvailableQuantity);
    }
    [Fact]
    public async Task Handle_When_Inventory_Item_Already_Exists_Should_Not_Create_Duplicate()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var repository =
            new InventoryItemRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new CreateInventoryItemCommandHandler(
                repository,
                unitOfWork);

        var productId = Guid.NewGuid();

        var command =
            new CreateInventoryItemCommand(productId);

        // Act
        await handler.Handle(
            command,
            CancellationToken.None);

        await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var count =
            await assertionDbContext.InventoryItems
                .CountAsync(x => x.ProductId == productId);

        Assert.Equal(1, count);
    }
}
