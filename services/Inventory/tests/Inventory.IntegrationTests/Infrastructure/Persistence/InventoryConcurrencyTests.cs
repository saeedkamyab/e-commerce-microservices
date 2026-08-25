using Inventory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventory.IntegrationTests.Infrastructure.Persistence;

[Collection(InventoryDatabaseCollection.Name)]
public sealed class InventoryConcurrencyTests
{
    private readonly InventoryDatabaseFixture _fixture;

    public InventoryConcurrencyTests(
        InventoryDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Concurrent_Reservations_Should_Detect_Concurrency_Conflict()
    {
        // Arrange
        var productId = Guid.NewGuid();

        await using (var setupDbContext =
                     _fixture.CreateDbContext())
        {
            var inventoryItem =
                InventoryItem.Create(productId);

            inventoryItem.IncreaseStock(10);

            setupDbContext.InventoryItems.Add(
                inventoryItem);

            await setupDbContext.SaveChangesAsync();
        }

        await using var dbContextA =
            _fixture.CreateDbContext();

        await using var dbContextB =
            _fixture.CreateDbContext();

   
        var itemA =
            await dbContextA.InventoryItems
                .SingleAsync(
                    x => x.ProductId == productId);

        var itemB =
            await dbContextB.InventoryItems
                .SingleAsync(
                    x => x.ProductId == productId);


        Assert.Equal(10, itemA.AvailableQuantity);
        Assert.Equal(10, itemB.AvailableQuantity);

        itemA.Reserve(6);
        itemB.Reserve(6);

        await dbContextA.SaveChangesAsync();

       
        var action = () =>
            dbContextB.SaveChangesAsync();

   
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            action);

       
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedItem =
            await assertionDbContext.InventoryItems
                .AsNoTracking()
                .SingleAsync(
                    x => x.ProductId == productId);

        Assert.Equal(10, persistedItem.Quantity);
        Assert.Equal(6, persistedItem.ReservedQuantity);
        Assert.Equal(4, persistedItem.AvailableQuantity);
    }
}
