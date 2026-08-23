using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.ChangeProductPrice;
using Catalog.Contracts.IntegrationEvents;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace Catalog.IntegrationTests.Application.Products;

[Collection(CatalogDatabaseCollection.Name)]
public sealed class ChangeProductPriceOutboxTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public ChangeProductPriceOutboxTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Price_And_Outbox_Message()
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

        // اگر Create خودش DomainEvent داشته باشد، برای تمیزی تست:
        product.ClearDomainEvents();

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

        // Assert با Context جدید
        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var persistedProduct =
            await assertionDbContext.Products
                .AsNoTracking()
                .SingleAsync(x => x.Id == product.Id);

        Assert.Equal(
            1350m,
            persistedProduct.Price.Amount);

        var outboxMessages =
      await assertionDbContext.OutboxMessages
          .AsNoTracking()
          .ToListAsync();


        var outboxMessage = Assert.Single(
            outboxMessages.Where(x =>
                x.Type ==
                typeof(ProductPriceChangedIntegrationEvent).FullName));

        Assert.NotNull(outboxMessage.Content);

        var integrationEvent =
            JsonSerializer.Deserialize<ProductPriceChangedIntegrationEvent>(
                outboxMessage.Content);

        Assert.NotNull(integrationEvent);

        Assert.Equal(product.Id, integrationEvent.ProductId);
        Assert.Equal(1200m, integrationEvent.OldPrice);
        Assert.Equal(1350m, integrationEvent.NewPrice);
        Assert.Equal("USD", integrationEvent.Currency);

        Assert.Null(outboxMessage.ProcessedOnUtc);
        Assert.Null(outboxMessage.Error);

        Assert.Empty(product.DomainEvents);



    }
}
