using Order.Application.Abstractions.Catalog;
using System.Net;
using System.Net.Http.Json;

namespace Order.Infrastructure.Catalog;

internal sealed class CatalogService : ICatalogService
{
    private readonly HttpClient _httpClient;

    public CatalogService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<CatalogProduct?> GetProductAsync(
        Guid productId,
        CancellationToken cancellationToken)
    {
        var response =
            await _httpClient.GetAsync(
                $"/api/products/{productId}",
                cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();

        var product =
            await response.Content
                .ReadFromJsonAsync<CatalogProductResponse>(
                    cancellationToken: cancellationToken);

        if (product is null)
        {
            throw new InvalidOperationException(
                "Catalog returned an empty product response.");
        }

        return new CatalogProduct(
            product.Id,
            product.Price,
            product.Currency,
            string.Equals(
                product.Status,
                "Active",
                StringComparison.OrdinalIgnoreCase));
    }

    private sealed record CatalogProductResponse(
        Guid Id,
        string Name,
        string? Description,
        Guid CategoryId,
        decimal Price,
        string Currency,
        string Status);
}
