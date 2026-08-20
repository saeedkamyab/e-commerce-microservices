using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.Application.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    string Name,
    Guid? ParentCategoryId,
    IReadOnlyCollection<CategoryAttributeDefinitionInput> Attributes
) : IRequest<Guid>;

public sealed record CategoryAttributeDefinitionInput(
    string Name,
    AttributeType Type,
    bool IsRequired,
    IReadOnlyCollection<string> Options);
