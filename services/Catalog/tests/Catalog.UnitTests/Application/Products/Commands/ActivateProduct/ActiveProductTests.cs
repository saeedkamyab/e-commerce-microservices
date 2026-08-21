using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Products.Commands.ActivateProduct;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.UnitTests.Application.Products.Commands.ActivateProduct;

public class ActiveProductTests
{
    [Fact]
    public async Task Handle_Should_Activate_Product()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("iPhone 17"),
            "",
            Guid.NewGuid(),
            Price.Create(1200m, "USD"));

        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                product.Id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new ActivateProductCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        // Act
        await handler.Handle(
            new ActivateProductCommand(product.Id),
            CancellationToken.None);

        // Assert
        Assert.Equal(ProductStatus.Active, product.Status);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
    [Fact]
    public async Task Handle_Should_Not_Save_When_Product_Does_Not_Exist()
    {
        // Arrange
        var productRepository = new Mock<IProductRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        productRepository
            .Setup(x => x.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new ActivateProductCommandHandler(
            productRepository.Object,
            unitOfWork.Object);

        // Act
        var action = () => handler.Handle(
            new ActivateProductCommand(Guid.NewGuid()),
            CancellationToken.None);

        // Assert
        await Assert.ThrowsAsync<InvalidOperationException>(action);

        unitOfWork.Verify(
            x => x.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}
