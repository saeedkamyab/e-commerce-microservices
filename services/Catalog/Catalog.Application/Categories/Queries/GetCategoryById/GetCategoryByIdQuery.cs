using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryById;

public sealed record GetCategoryByIdQuery(
    Guid CategoryId
) : IRequest<CategoryDetailsResponse?>;
