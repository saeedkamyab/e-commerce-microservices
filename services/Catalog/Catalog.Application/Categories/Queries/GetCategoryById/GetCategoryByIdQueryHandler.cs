using Catalog.Application.Abstractions.Persistence;
using MediatR;

namespace Catalog.Application.Categories.Queries.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, CategoryDetailsResponse?>
{
    private readonly ICategoryRepository _categoryRepository;

    public GetCategoryByIdQueryHandler(
        ICategoryRepository categoryRepository)
    {
        _categoryRepository = categoryRepository;
    }

    public async Task<CategoryDetailsResponse?> Handle(
        GetCategoryByIdQuery request,
        CancellationToken cancellationToken)
    {
        var category = await _categoryRepository.GetByIdAsync(
            request.CategoryId,
            cancellationToken);

        if (category is null)
            return null;

        return new CategoryDetailsResponse(
            category.Id,
            category.Name.Value,
            category.ParentCategoryId,
            category.Status.ToString(),
            category.AttributeDefinitions
                .Select(x =>
                    new CategoryAttributeResponse(
                        x.Id,
                        x.Name,
                        x.Type,
                        x.IsRequired,
                        x.Options
                            .Select(o => o.Value)
                            .ToArray()))
                .ToArray());
    }
}
