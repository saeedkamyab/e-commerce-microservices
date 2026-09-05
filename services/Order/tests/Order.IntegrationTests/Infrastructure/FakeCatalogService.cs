using Order.Application.Abstractions.Catalog;

namespace Order.IntegrationTests.Infrastructure;

public sealed class FakeCatalogService : ICatalogService
{
    private readonly Dictionary<Guid, CatalogProduct> _products = new();
    public void AddProduct(
    Guid productId,
    decimal price,
    string currency = "USD",
    bool isActive = true)
    {
        _products[productId] =
            new CatalogProduct(
                productId,
                price,
                currency,
                isActive);
    }
    public Task<CatalogProduct?> GetProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        _products.TryGetValue(
               productId,
               out var product);

        return Task.FromResult(product);

    }

}

