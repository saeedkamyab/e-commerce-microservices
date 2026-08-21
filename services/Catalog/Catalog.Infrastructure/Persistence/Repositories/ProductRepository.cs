using Catalog.Application.Abstractions.Persistence;
using Catalog.Domain.Entities;
using Catalog.Domain.Enums;
using Catalog.Domain.ValueObjects;
using Catalog.Infrastructure.Persistence.Models;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Repositories;

internal sealed class ProductRepository : IProductRepository
{
    private readonly CatalogDbContext _dbContext;

    public ProductRepository(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Product?> GetByIdAsync(
    Guid id,
    CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (product is null)
            return null;

        var specificationRecords =
            await _dbContext.ProductSpecifications
                .AsNoTracking()
                .Where(x => x.ProductId == id)
                .ToListAsync(cancellationToken);

        foreach (var record in specificationRecords)
        {
            var value = MapSpecificationValue(record);

            var specification =
                ProductSpecification.Rehydrate(
                    record.Id,
                    record.AttributeDefinitionId,
                    value);

            product.AddSpecification(specification);
        }

        return product;
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

    private static ProductSpecificationValue MapSpecificationValue(
    ProductSpecificationRecord record)
    {
        if (record.TextValue is not null)
        {
            return ProductSpecificationValue.CreateText(
                record.TextValue);
        }

        if (record.NumberValue.HasValue)
        {
            return ProductSpecificationValue.CreateNumber(
                record.NumberValue.Value);
        }

        if (record.DecimalValue.HasValue)
        {
            return ProductSpecificationValue.CreateDecimal(
                record.DecimalValue.Value);
        }

        if (record.BooleanValue.HasValue)
        {
            return ProductSpecificationValue.CreateBoolean(
                record.BooleanValue.Value);
        }

        if (record.DateValue.HasValue)
        {
            return ProductSpecificationValue.CreateDate(
                record.DateValue.Value);
        }

        if (record.OptionValue is not null)
        {
            return ProductSpecificationValue.CreateOption(
                AttributeOption.Create(record.OptionValue));
        }

        throw new InvalidOperationException(
            $"Specification '{record.Id}' does not contain a value.");
    }


}
