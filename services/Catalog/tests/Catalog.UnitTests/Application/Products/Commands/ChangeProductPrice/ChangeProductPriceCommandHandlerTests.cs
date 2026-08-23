using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.ChangeProductPrice;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.UnitTests.Application.Products.Commands.ChangeProductPrice;

public class ChangeProductPriceCommandHandlerTests
{
    [Fact]
    public async Task Handle_Should_Change_Product_Price()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("iPhone 17"),
            "Test product",
            Guid.NewGuid(),
            Price.Create(1200m, "USD"));

        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new ChangeProductPriceCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        // Act
        await handler.Handle(
            new ChangeProductPriceCommand(
                product.Id,
                1350m,
                "USD"),
            CancellationToken.None);

        // Assert
        Assert.Equal(1350m, product.Price.Amount);
        Assert.Equal("USD", product.Price.Currency);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Handle_Should_Not_Save_When_Product_Does_Not_Exist()
    {
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new ChangeProductPriceCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        var action = () => handler.Handle(
            new ChangeProductPriceCommand(
                Guid.NewGuid(),
                1350m,
                "USD"),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
