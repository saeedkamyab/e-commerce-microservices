using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.CreateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Moq;
using System.Timers;

namespace Catalog.UnitTests.Application.Products.Commands.CreateProduct;

public class CreateProductTests
{
    [Fact]
    public async Task Handle_Should_Create_Product_When_Category_Exists()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
            "iPhone 17",
            "This is a test",
            category.Id,
            1200m,
            "USD",
            []);

        // Act
        var productId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, productId);

        productRepository.Verify(
            x => x.AddAsync(
                It.Is<Product>(p =>
                    p.Id == productId &&
                    p.CategoryId == category.Id &&
                    p.Name == ProductName.Create("iPhone 17")),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Handle_Should_Throw_When_Category_Does_Not_Exist()
    {
        // Arrange
        var categoryId = Guid.NewGuid();

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                categoryId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
            "iPhone 17",
            "This is a test",
            categoryId,
            1200m,
            "USD",
            []);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_Should_Throw_When_Attribute_Does_Not_Belong_To_Category()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
           "iPhone 17",
 "This is a test",
 category.Id,
 1200m,
 "USD",
            [
                new ProductSpecificationInput(
                Guid.NewGuid(),
                "8")
            ]);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_Should_Throw_When_Required_Attribute_Is_Missing()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var ramDefinition =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        category.AddAttributeDefinition(ramDefinition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
              "iPhone 17",
 "This is a test",
 category.Id,
 1200m,
 "USD",
            []);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Fact]
    public async Task Handle_Should_Create_Product_When_Required_Attributes_Are_Provided()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var ramDefinition =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        category.AddAttributeDefinition(ramDefinition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
            "iPhone 17",
 "This is a test",
 category.Id,
 1200m,
 "USD",
            [
                new ProductSpecificationInput(
                ramDefinition.Id,
                "8")
            ]);

        // Act
        var productId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, productId);

        productRepository.Verify(
            x => x.AddAsync(
                It.Is<Product>(p =>
                    p.Specifications.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Throw_When_Number_Attribute_Has_Invalid_Value()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var ramDefinition =
            CategoryAttributeDefinition.Create(
                "RAM",
                AttributeType.Number,
                true);

        category.AddAttributeDefinition(ramDefinition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
            "iPhone 17",
            "This is a test",
            category.Id,
            1200m,
            "USD",
            [
                new ProductSpecificationInput(
                ramDefinition.Id,
                "not-a-number")
            ]);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
    [Theory]
    [InlineData("true")]
    [InlineData("false")]
    public async Task Handle_Should_Accept_Valid_Boolean_Value(
    string value)
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var definition =
            CategoryAttributeDefinition.Create(
                "5G",
                AttributeType.Boolean,
                true);

        category.AddAttributeDefinition(definition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
                    "iPhone 17",
"This is a test",
category.Id,
1000m,
"USD",
            [
                new ProductSpecificationInput(
                definition.Id,
                value)
            ]);

        // Act
        var result = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, result);
    }
    [Fact]
    public async Task Handle_Should_Throw_When_Boolean_Attribute_Has_Invalid_Value()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var definition =
            CategoryAttributeDefinition.Create(
                "5G",
                AttributeType.Boolean,
                true);

        category.AddAttributeDefinition(definition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
                    "iPhone 17",
"This is a test",
category.Id,
1200m,
"USD",
            [
                new ProductSpecificationInput(
                definition.Id,
                "yes")
            ]);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<ArgumentException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Accept_Valid_Option_Value()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var colorDefinition =
            CategoryAttributeDefinition.Create(
                "Color",
                AttributeType.Option,
                true);

        colorDefinition.AddOption(
            AttributeOption.Create("Black"));

        colorDefinition.AddOption(
            AttributeOption.Create("White"));

        category.AddAttributeDefinition(colorDefinition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
                   "iPhone 17",
"This is a test",
category.Id,
1200m,
"USD",
            [
                new ProductSpecificationInput(
                colorDefinition.Id,
                "Black")
            ]);

        // Act
        var productId = await handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        Assert.NotEqual(Guid.Empty, productId);

        productRepository.Verify(
            x => x.AddAsync(
                It.Is<Product>(p =>
                    p.Specifications.Count == 1),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Handle_Should_Throw_When_Option_Value_Is_Not_Allowed()
    {
        // Arrange
        var category = Category.Create(
            CategoryName.Create("Mobile"));

        var colorDefinition =
            CategoryAttributeDefinition.Create(
                "Color",
                AttributeType.Option,
                true);

        colorDefinition.AddOption(
            AttributeOption.Create("Black"));

        colorDefinition.AddOption(
            AttributeOption.Create("White"));

        category.AddAttributeDefinition(colorDefinition);

        var categoryRepository = new Mock<ICategoryRepository>();
        var productRepository = new Mock<IProductRepository>();

        categoryRepository
            .Setup(x => x.GetByIdAsync(
                category.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(category);

        var handler = new CreateProductCommandHandler(
            categoryRepository.Object,
            productRepository.Object);

        var command = new CreateProductCommand(
                           "iPhone 17",
"This is a test",
category.Id,
1200m,
"USD",
            [
                new ProductSpecificationInput(
                colorDefinition.Id,
                "Pink")
            ]);

        // Act
        var action = () => handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);

        productRepository.Verify(
            x => x.AddAsync(
                It.IsAny<Product>(),
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
