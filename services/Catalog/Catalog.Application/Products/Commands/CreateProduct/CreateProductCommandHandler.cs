using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using MediatR;

namespace Catalog.Application.Products.Commands.CreateProduct;

public sealed class CreateProductCommandHandler
    : IRequestHandler<CreateProductCommand, Guid>
{
    private readonly ICategoryRepository _categoryRepository;
    private readonly IProductRepository _productRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProductCommandHandler(
        ICategoryRepository categoryRepository,
        IProductRepository productRepository, 
        IUnitOfWork unitOfWork)
    {
        _categoryRepository = categoryRepository;
        _productRepository = productRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> Handle(
        CreateProductCommand request,
        CancellationToken cancellationToken)
    {
        var category =
            await _categoryRepository.GetByIdAsync(
                request.CategoryId,
                cancellationToken);

        if (category is null)
        {
            throw new InvalidOperationException(
                "Category was not found.");
        }

        var requestedAttributeIds = request.Specifications.Select(x => x.AttributeDefinitionId).ToHashSet();

        var missingRequiredAttribute = category.AttributeDefinitions
    .FirstOrDefault(x =>
        x.IsRequired &&
        !requestedAttributeIds.Contains(x.Id));

        if (missingRequiredAttribute is not null)
        {
            throw new InvalidOperationException(
                $"Required attribute '{missingRequiredAttribute.Name}' was not provided.");
        }


        var productName =
            ProductName.Create(request.Name);

        var price =
            Price.Create(request.Price, request.Currency);

        var product =
            Product.Create(
                productName,
                request.Description,
                category.Id,
                price);


        foreach (var input in request.Specifications)
        {
            var definition =
                category.AttributeDefinitions
                    .FirstOrDefault(x =>
                        x.Id == input.AttributeDefinitionId);

            if (definition is null)
            {
                throw new InvalidOperationException(
                    "Attribute definition does not belong to category.");
            }
            if (definition.Type == AttributeType.Option)
            {
                var option = definition.Options
                    .FirstOrDefault(x =>
                        x.Value.Equals(
                            input.Value,
                            StringComparison.OrdinalIgnoreCase));

                if (option is null)
                {
                    throw new InvalidOperationException(
                        $"'{input.Value}' is not a valid option for attribute '{definition.Name}'.");
                }

                product.AddSpecification(
            ProductSpecification.Create(
                definition.Id,
                ProductSpecificationValue.CreateOption(option)));


                continue;
            }

            var value =
                ProductSpecificationValueFactory.Create(
                    definition.Type,
                    input.Value);

            var specification =
                ProductSpecification.Create(
                    definition.Id,
                    value);

            product.AddSpecification(specification);
        }


        await _productRepository.AddAsync(
            product,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
    cancellationToken);


        return product.Id;
    }
}
