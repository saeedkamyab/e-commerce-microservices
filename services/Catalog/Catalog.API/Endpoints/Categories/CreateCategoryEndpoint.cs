using Catalog.Application.Categories.Commands.CreateCategory;
using Catalog.Domain.Enums;
using MediatR;

namespace Catalog.API.Endpoints.Categories;

public static class CreateCategoryEndpoint
{
    public static IEndpointRouteBuilder MapCreateCategoryEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost(
            "/api/categories",
            async (
                CreateCategoryRequest request,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var command = new CreateCategoryCommand(
                    request.Name,
                    request.ParentCategoryId,
                    request.Attributes
                        .Select(x =>
                            new CategoryAttributeDefinitionInput(
                                x.Name,
                                x.Type,
                                x.IsRequired,
                                x.Options))
                        .ToArray());

                var categoryId = await sender.Send(
                    command,
                    cancellationToken);

                return Results.Created(
                    $"/api/categories/{categoryId}",
                    new { Id = categoryId });
            });

        return endpoints;
    }
}

public sealed record CreateCategoryRequest(
    string Name,
    Guid? ParentCategoryId,
    IReadOnlyCollection<CategoryAttributeDefinitionRequest> Attributes);

public sealed record CategoryAttributeDefinitionRequest(
    string Name,
    AttributeType Type,
    bool IsRequired,
    IReadOnlyCollection<string> Options);
