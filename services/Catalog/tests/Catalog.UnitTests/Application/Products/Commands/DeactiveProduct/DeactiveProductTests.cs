using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.DeactivateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.UnitTests.Application.Products.Commands.DeactiveProduct;

public class DeactiveProductTests
{
    [Fact]
    public async Task Handle_Should_Deactivate_Product()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("iPhone 17"),
            "",
            Guid.NewGuid(),
            Price.Create(1200m, "USD"));

        product.Activate();

        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new DeactivateProductCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        // Act
        await handler.Handle(
            new DeactivateProductCommand(product.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(ProductStatus.Inactive, product.Status);

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

        var handler = new DeactivateProductCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        var action = () => handler.Handle(
            new DeactivateProductCommand(Guid.NewGuid()),
            CancellationToken.None);

        await Assert.ThrowsAsync<InvalidOperationException>(action);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
