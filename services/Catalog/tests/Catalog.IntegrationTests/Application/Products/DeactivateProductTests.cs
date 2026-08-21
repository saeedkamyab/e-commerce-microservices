using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.DeactivateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.Application.Products;

[Collection(CatalogDatabaseCollection.Name)]
public sealed class DeactivateProductTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public DeactivateProductTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Inactive_Status()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var category = Category.Create(
            CategoryName.Create($"Mobile-{Guid.NewGuid()}"));

        dbContext.Categories.Add(category);

        var product = Product.Create(
            ProductName.Create("iPhone 17"),
            "Test product",
            category.Id,
            Price.Create(1200m, "USD"));

        product.Activate();

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        var productId = product.Id;

        Assert.Equal(
            ProductStatus.Active,
            product.Status);

        var productRepository =
            new ProductRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new DeactivateProductCommandHandler(
                productRepository,
                unitOfWork);

        // Act
        await handler.Handle(
            new DeactivateProductCommand(productId),
            CancellationToken.None);

        // Assert
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedProduct =
            await assertionDbContext.Products
                .AsNoTracking()
                .SingleAsync(x => x.Id == productId);

        Assert.Equal(
            ProductStatus.Inactive,
            persistedProduct.Status);
    }
}
