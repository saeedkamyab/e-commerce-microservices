using Catalog.Application.Abstractions.Persistence.Queries;
using Catalog.Application.Products.Queries.GetProductById;
using Microsoft.EntityFrameworkCore;

namespace Catalog.Infrastructure.Persistence.Queries;

internal sealed class ProductReadService
    : IProductReadService
{
    private readonly CatalogDbContext _dbContext;

    public ProductReadService(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ProductDetailsResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .Where(x => x.Id == productId)
            .Select(x => new
            {
                x.Id,
                Name = x.Name.Value,
                x.Description,
                x.CategoryId,
                Price = x.Price.Amount,
                Currency = x.Price.Currency,
                Status = x.Status.ToString()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (product is null)
            return null;

        var specifications = await _dbContext
            .ProductSpecifications
            .AsNoTracking()
            .Where(x => x.ProductId == productId)
            .Select(x => new ProductSpecificationResponse(
                x.AttributeDefinitionId,

                x.TextValue
                ?? (x.NumberValue != null
                    ? x.NumberValue.Value.ToString()
                    : null)
                ?? (x.DecimalValue != null
                    ? x.DecimalValue.Value.ToString()
                    : null)
                ?? (x.BooleanValue != null
                    ? x.BooleanValue.Value.ToString()
                    : null)
                ?? (x.DateValue != null
                    ? x.DateValue.Value.ToString()
                    : null)
                ?? x.OptionValue
                ?? string.Empty
            ))
            .ToArrayAsync(cancellationToken);

        return new ProductDetailsResponse(
            product.Id,
            product.Name,
            product.Description,
            product.CategoryId,
            product.Price,
            product.Currency,
            product.Status,
            specifications);
    }
}
