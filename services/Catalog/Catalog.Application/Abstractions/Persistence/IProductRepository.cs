using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions.Persistence;
public interface IProductRepository
{
    Task AddAsync(
        Product product,
        CancellationToken cancellationToken);
}
