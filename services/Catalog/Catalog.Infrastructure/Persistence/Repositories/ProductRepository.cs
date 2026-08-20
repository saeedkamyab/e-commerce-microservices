using Catalog.Domain.Enums;
using Catalog.Domain.Entities;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Models;
using Catalog.Application.Abstractions.Persistence;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Product product,
        CancellationToken cancellationToken)
    {
        await _dbContext.Products.AddAsync(
            product,
            cancellationToken);

        foreach (var specification in product.Specifications)
        {
            var record = MapSpecification(
                product.Id,
                specification);

            await _dbContext.ProductSpecifications.AddAsync(
                record,
                cancellationToken);
        }
    }

    private static ProductSpecificationRecord MapSpecification(
        Guid productId,
        ProductSpecification specification)
    {
        var record = new ProductSpecificationRecord
        {
            Id = specification.Id,
            ProductId = productId,
            AttributeDefinitionId =
                specification.AttributeDefinitionId
        };

        switch (specification.Value.Type)
        {
            case AttributeType.Text:
                record.TextValue =
                    (string)specification.Value.Value;
                break;

            case AttributeType.Number:
                record.NumberValue =
                    (int)specification.Value.Value;
                break;

            case AttributeType.Decimal:
                record.DecimalValue =
                    (decimal)specification.Value.Value;
                break;

            case AttributeType.Boolean:
                record.BooleanValue =
                    (bool)specification.Value.Value;
                break;

            case AttributeType.Date:
                record.DateValue =
                    (DateTime)specification.Value.Value;
                break;

            case AttributeType.Option:
                var option =
                    (AttributeOption)specification.Value.Value;

                record.OptionValue = option.Value;
                break;

            default:
                throw new InvalidOperationException(
                    $"Unsupported specification type: {specification.Value.Type}");
        }

        return record;
    }
}
