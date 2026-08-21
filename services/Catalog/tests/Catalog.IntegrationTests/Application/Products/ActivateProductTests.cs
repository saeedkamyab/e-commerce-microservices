using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.ActivateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.Application.Products;

[Collection(CatalogDatabaseCollection.Name)]
public class ActivateProductTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public ActivateProductTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Active_Status()
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

        dbContext.Products.Add(product);

        await dbContext.SaveChangesAsync();

        var productId = product.Id;

        Assert.Equal(
            ProductStatus.Draft,
            product.Status);

        var productRepository =
            new ProductRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new ActivateProductCommandHandler(
                productRepository,
                unitOfWork);

        // Act
        await handler.Handle(
            new ActivateProductCommand(productId),
            CancellationToken.None);

        // Assert

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedProduct =
            await assertionDbContext.Products
                .AsNoTracking()
                .SingleAsync(
                    x => x.Id == productId);

        Assert.Equal(
            ProductStatus.Active,
            persistedProduct.Status);
    }
}
