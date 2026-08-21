using Catalog.Application.Products.Queries.GetProductById;

namespace Catalog.Application.Abstractions.Persistence.Queries;

public interface IProductReadService
{
    Task<ProductDetailsResponse?> GetByIdAsync(
        Guid productId,
        CancellationToken cancellationToken);
}
