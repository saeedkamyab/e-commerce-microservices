using Catalog.Domain.Enums;

namespace Catalog.Application.Categories.Queries.GetCategoryById;

public sealed record CategoryDetailsResponse(
    Guid Id,
    string Name,
    Guid? ParentCategoryId,
    string Status,
    IReadOnlyCollection<CategoryAttributeResponse> Attributes);

public sealed record CategoryAttributeResponse(
    Guid Id,
    string Name,
    AttributeType Type,
    bool IsRequired,
    IReadOnlyCollection<string> Options);
