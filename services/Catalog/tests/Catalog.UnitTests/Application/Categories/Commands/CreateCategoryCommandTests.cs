using Catalog.Application.Abstractions.Persistence;
using Catalog.Application.Categories.Commands.CreateCategory;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Moq;

namespace Catalog.UnitTests.Application.Categories.Commands;

public class CreateCategoryCommandTests
{
    [Fact]
    public async Task Handle_ShouldCreateCategory_WhenValidRequest()
    {
        // Arrange
        var categoryRepository = new Mock<ICategoryRepository>();
        var unitOfWork = new Mock<IUnitOfWork>();

        var handler = new CreateCategoryCommandHandler(
            categoryRepository.Object,
            unitOfWork.Object);

        var command = new CreateCategoryCommand(
            "Test Category",
            null,
            new List<CategoryAttributeDefinitionInput>
            {
                new CategoryAttributeDefinitionInput(
                    "Color",
                    AttributeType.Option,
                    true,
                    new List<string> { "Red", "Green", "Blue" })
            });

        // Act
        var categoryId = await handler.Handle(command, CancellationToken.None);
       
        // Assert
        Assert.NotEqual(Guid.Empty, categoryId);
       

        categoryRepository.Verify(
            x => x.AddAsync(It.Is<Category>(
                x => x.Id == categoryId
                && x.Name == CategoryName.Create("Test Category")
                && x.ParentCategoryId == null
                ),
            It.IsAny<CancellationToken>()), Times.Once);
        
        unitOfWork.Verify(
            u => u.SaveChangesAsync(
                It.IsAny<CancellationToken>()), Times.Once);
    }
}
