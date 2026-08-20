using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Models;
using Catalog.Infrastructure.Persistence.Repositories;
using Catalog.IntegrationTests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace Catalog.IntegrationTests.Application.Products;

[Collection(CatalogDatabaseCollection.Name)]
public sealed class CreateProductTests
{
    private readonly CatalogDatabaseFixture _fixture;

    public CreateProductTests(
        CatalogDatabaseFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Handle_Should_Persist_Product_With_Specifications()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var ramDefinition =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        category.AddAttributeDefinition(ramDefinition);



        dbContext.Categories.Add(category);

        dbContext.CategoryAttributeDefinitions.Add(
            new CategoryAttributeDefinitionRecord
            {
                Id = ramDefinition.Id,
                CategoryId = category.Id,
                Name = ramDefinition.Name,
                Type = ramDefinition.Type,
                IsRequired = ramDefinition.IsRequired
            });

        await dbContext.SaveChangesAsync();

        var categoryRepository =
    new CategoryRepository(dbContext);

        var productRepository =
            new ProductRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new CreateProductCommandHandler(
                categoryRepository,
                productRepository,
                unitOfWork);


        var command =
    new CreateProductCommand(
        "iPhone 17",
        "",
        category.Id,
        1200m,
        "USD",
        [
            new ProductSpecificationInput(
                ramDefinition.Id,
                "8")
        ]);

        var productId = await handler.Handle(
    command,
    CancellationToken.None);

        await using var assertionDbContext =
       _fixture.CreateDbContext();

        var product = await assertionDbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(
                x => x.Id == productId);

        Assert.NotNull(product);

        Assert.Equal(
            "iPhone 17",
            product.Name.Value);

        Assert.Equal(
            category.Id,
            product.CategoryId);

        Assert.Equal(
            ProductStatus.Draft,
            product.Status);

        var specification = await assertionDbContext
     .ProductSpecifications
     .AsNoTracking()
     .SingleAsync(
         x => x.ProductId == productId);

        Assert.Equal(
            ramDefinition.Id,
            specification.AttributeDefinitionId);

        Assert.Equal(
            8,
            specification.NumberValue);

    }

    [Fact]
    public async Task Handle_Should_Not_Persist_Product_When_Specification_Value_Is_Invalid()
    {
        // Arrange
        await using var dbContext =
            _fixture.CreateDbContext();

        var category = Category.Create(
            CategoryName.Create("Laptop"));

        var ramDefinition =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        category.AddAttributeDefinition(ramDefinition);

        dbContext.Categories.Add(category);

        dbContext.CategoryAttributeDefinitions.Add(
            new CategoryAttributeDefinitionRecord
            {
                Id = ramDefinition.Id,
                CategoryId = category.Id,
                Name = ramDefinition.Name,
                Type = ramDefinition.Type,
                IsRequired = ramDefinition.IsRequired
            });

        await dbContext.SaveChangesAsync();

        var categoryRepository =
            new CategoryRepository(dbContext);

        var productRepository =
            new ProductRepository(dbContext);

        IUnitOfWork unitOfWork = dbContext;

        var handler =
            new CreateProductCommandHandler(
                categoryRepository,
                productRepository,
                unitOfWork);

        var command =
            new CreateProductCommand(
                "MacBook Pro",
                "",
                category.Id,
                2000m,
                "USD",
                [
                    new ProductSpecificationInput(
                    ramDefinition.Id,
                    "invalid-number")
                ]);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        await using var assertionDbContext =
            _fixture.CreateDbContext();

        var productExists =
            await assertionDbContext.Products
                .AnyAsync(x =>
                    x.Name.Value == "MacBook Pro");

        Assert.False(productExists);
    }

}
