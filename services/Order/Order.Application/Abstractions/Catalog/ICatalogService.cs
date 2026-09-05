namespace Order.Application.Abstractions.Catalog;

public interface ICatalogService
{
    Task<CatalogProduct?> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken);
}

public sealed record CatalogProduct(
    Guid Id,
    decimal Price,
    string Currency,
    bool IsActive);
