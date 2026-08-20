using Catalog.Application.Categories.Queries.GetCategoryById;
using MediatR;

namespace Catalog.API.Endpoints.Categories;

public static class GetCategoryByIdEndpoint
{
    public static IEndpointRouteBuilder MapGetCategoryByIdEndpoint(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            "/api/categories/{id:guid}",
            async (
                Guid id,
                ISender sender,
                CancellationToken cancellationToken) =>
            {
                var result = await sender.Send(
                    new GetCategoryByIdQuery(id),
                    cancellationToken);

                return result is null
                    ? Results.NotFound()
                    : Results.Ok(result);
            });

        return endpoints;
    }
}
