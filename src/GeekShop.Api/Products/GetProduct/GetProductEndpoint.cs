namespace GeekShop.Api.Products.GetProduct;

internal static class GetProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/{id:guid}", async (
            GetProductUseCase useCase,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var product = await useCase.HandleAsync(id, cancellationToken);

            if(product is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(product);
        }).WithName("GetProduct");
}
