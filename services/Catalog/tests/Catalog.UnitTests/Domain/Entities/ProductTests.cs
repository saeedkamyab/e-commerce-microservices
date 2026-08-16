using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.Events;
using Catalog.Domain.ValueObjects;

namespace Catalog.UnitTests.Domain.Entities;

public class ProductTests
{
    private static Product CreateProduct()
    {
        return Product.Create(
            ProductName.Create("iPhone 17"),
            "TestProduct",
            Guid.NewGuid(),
            Price.Create(1000, "USD"));
    }
    [Fact]
    public void Create_Should_Create_Product_As_Draft()
    {
        // Act
        var product = CreateProduct();

        // Assert
        Assert.Equal(ProductStatus.Draft, product.Status);
    }

    [Fact]
    public void Activate_Should_Change_Status_To_Active()
    {
        var product = CreateProduct();
        // Act
        product.Activate();

        // Assert
        Assert.Equal(ProductStatus.Active, product.Status);
    }
    [Fact]
    public void Deactivate_Should_Change_Active_Product_To_Inactive()
    {
        // Arrange
        var product = CreateProduct();
        product.Activate();

        // Act
        product.Deactivate();

        // Assert
        Assert.Equal(ProductStatus.Inactive, product.Status);
    }
    [Fact]
    public void Activate_Should_Change_Inactive_Product_To_Active()
    {
        // Arrange
        var product = CreateProduct();

        product.Activate();
        product.Deactivate();

        // Act
        product.Activate();

        // Assert
        Assert.Equal(ProductStatus.Active, product.Status);
    }
    [Fact]
    public void Deactivate_Should_Throw_When_Product_Is_Draft()
    {
        // Arrange
        var product = CreateProduct();

        // Act
        var action = () => product.Deactivate();

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }

    [Fact]
    public void Activate_Should_Throw_When_Price_Is_Zero()
    {
        // Arrange
        var product = Product.Create(
            ProductName.Create("Test Product"),
            null,
            Guid.NewGuid(),
            Price.Create(0, "USD"));

        // Act
        var action = () => product.Activate();

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }
    [Fact]
    public void ChangePrice_Should_Raise_ProductPriceChangedDomainEvent()
    {
        // Arrange
        var product = CreateProduct();

        var newPrice = Price.Create(1500, "USD");

        // Act
        product.ChangePrice(newPrice);

        // Assert
        var domainEvent = Assert.Single(product.DomainEvents);

        var priceChangedEvent =
            Assert.IsType<ProductPriceChangedDomainEvent>(domainEvent);

        Assert.Equal(product.Id, priceChangedEvent.ProductId);
        Assert.Equal(1000, priceChangedEvent.OldPrice);
        Assert.Equal(1500, priceChangedEvent.NewPrice);
    }
    [Fact]
    public void ChangePrice_With_SamePrice_Should_Not_Raise_Event()
    {
        // Arrange
        var product = CreateProduct();

        var samePrice = Price.Create(1000, "USD");

        // Act
        product.ChangePrice(samePrice);

        // Assert
        Assert.Empty(product.DomainEvents);
    }
    [Fact]
    public void AddSpecification_Should_Add_Specification()
    {
        // Arrange
        var product = CreateProduct();

        var attributeDefinitionId = Guid.NewGuid();

        var specification =
            ProductSpecification.Create(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(8));

        // Act
        product.AddSpecification(specification);

        // Assert
        var result = Assert.Single(
            product.Specifications);

        Assert.Equal(
            attributeDefinitionId,
            result.AttributeDefinitionId);

        Assert.Equal(
            8,
            result.Value.Value);
    }
    [Fact]
    public void AddSpecification_Should_Reject_Duplicate_Attribute()
    {
        // Arrange
        var product = CreateProduct();

        var attributeDefinitionId = Guid.NewGuid();

        product.AddSpecification(
            ProductSpecification.Create(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(8)));

        var duplicate =
            ProductSpecification.Create(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(12));

        // Act
        var action = () =>
            product.AddSpecification(duplicate);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }
    [Fact]
    public void ChangeSpecification_Should_Update_Value()
    {
        // Arrange
        var product = CreateProduct();

        var attributeDefinitionId = Guid.NewGuid();

        product.AddSpecification(
            ProductSpecification.Create(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(8)));

        // Act
        product.ChangeSpecification(
            attributeDefinitionId,
            ProductSpecificationValue.CreateNumber(12));

        // Assert
        var specification =
            Assert.Single(product.Specifications);

        Assert.Equal(
            12,
            specification.Value.Value);
    }
    [Fact]
    public void ChangeSpecification_Should_Throw_When_Specification_Does_Not_Exist()
    {
        // Arrange
        var product = CreateProduct();

        var attributeDefinitionId = Guid.NewGuid();

        // Act
        var action = () =>
            product.ChangeSpecification(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(12));

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }
    [Fact]
    public void RemoveSpecification_Should_Remove_Specification()
    {
        // Arrange
        var product = CreateProduct();

        var attributeDefinitionId = Guid.NewGuid();

        product.AddSpecification(
            ProductSpecification.Create(
                attributeDefinitionId,
                ProductSpecificationValue.CreateNumber(8)));

        // Act
        product.RemoveSpecification(attributeDefinitionId);

        // Assert
        Assert.Empty(product.Specifications);
    }
}
