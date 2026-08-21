using Catalog.Application.Products.Queries.GetProductById;
using MediatR;

namespace Catalog.API.Endpoints.Products
{
    public static class GetProductByIdEndpoint
    {
        public static IEndpointRouteBuilder MapGetProductByIdEndpoint(
            this IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet(
                "/api/products/{id:guid}",
                async (
                    Guid id,
                    ISender sender,
                    CancellationToken cancellationToken) =>
                {
                    var result = await sender.Send(
                        new GetProductByIdQuery(id),
                        cancellationToken);

                    return result is null
                        ? Results.NotFound()
                        : Results.Ok(result);
                });

            return endpoints;
        }
    }
}
