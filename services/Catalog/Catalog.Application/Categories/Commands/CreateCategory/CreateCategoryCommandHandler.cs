using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler
    : IRequestHandler<CreateCategoryCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCategoryCommandHandler(
        ICategoryRepository categoryRepository,
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateCategoryCommand request,
        CancellationToken cancellationToken)
    {
        if (request.ParentCategoryId.HasValue)
        {
            var parent = await _categoryRepository.GetByIdAsync(
                request.ParentCategoryId.Value,
                cancellationToken);

            if (parent is null)
            {
                throw new InvalidOperationException(
                    "Parent category was not found.");
            }
        }

        var category = Category.Create(
            CategoryName.Create(request.Name),
            request.ParentCategoryId);

        foreach (var attributeInput in request.Attributes)
        {
            var definition =
                CategoryAttributeDefinition.Create(
                    attributeInput.Name,
                    attributeInput.Type,
                    attributeInput.IsRequired);

            if (attributeInput.Type == AttributeType.Option)
            {
                if (attributeInput.Options.Count == 0)
                {
                    throw new InvalidOperationException(
                        $"Option attribute '{attributeInput.Name}' must have at least one option.");
                }

                foreach (var option in attributeInput.Options)
                {
                    definition.AddOption(
                        AttributeOption.Create(option));
                }
            }
            else if (attributeInput.Options.Count > 0)
            {
                throw new InvalidOperationException(
                    $"Attribute '{attributeInput.Name}' cannot have options because it is not of type Option.");
            }

            category.AddAttributeDefinition(definition);
        }

        await _categoryRepository.AddAsync(
            category,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return category.Id;
    }
}
