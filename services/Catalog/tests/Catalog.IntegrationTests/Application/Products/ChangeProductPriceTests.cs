using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.ChangeProductPrice;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.Application.Products;


    [Collection(CatalogDatabaseCollection.Name)]
    public sealed class ChangeProductPriceTests
    {
        private readonly CatalogDatabaseFixture _fixture;

        public ChangeProductPriceTests(
            CatalogDatabaseFixture fixture)
        {
            _fixture = fixture;
        }

        [Fact]
        public async Task Handle_Should_Persist_New_Price()
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

            var productRepository =
                new ProductRepository(dbContext);

            IUnitOfWork unitOfWork = dbContext;

            var handler =
                new ChangeProductPriceCommandHandler(
                    productRepository,
                    unitOfWork);

            // Act
            await handler.Handle(
                new ChangeProductPriceCommand(
                    product.Id,
                    1350m,
                    "USD"),
                CancellationToken.None);

            // Assert
            await using var assertionDbContext =
                _fixture.CreateDbContext();

            var persistedProduct =
                await assertionDbContext.Products
                    .AsNoTracking()
                    .SingleAsync(x => x.Id == product.Id);

            Assert.Equal(
                1350m,
                persistedProduct.Price.Amount);

            Assert.Equal(
                "USD",
                persistedProduct.Price.Currency);
        }
    }

