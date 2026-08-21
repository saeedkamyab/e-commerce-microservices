using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions.Persistence;
public interface IProductRepository
{

    Task<Product?> GetByIdAsync(
       Guid id,
       CancellationToken cancellationToken);

    Task AddAsync(
        Product product,
        CancellationToken cancellationToken);


}
