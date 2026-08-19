using Catalog.Domain.Entities;

namespace Catalog.Application.Abstractions.Persistence;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(
        Guid id,
        CancellationToken cancellationToken);

  
}