namespace GeekShop.Api.Products.GetProduct;

internal static class GetProductEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/{id:guid}", async (
            GetProductHandler handler,
            Guid id,
            CancellationToken cancellationToken) =>
        {
            var product = await handler.HandleAsync(id, cancellationToken);

            if(product is null)
            {
                return Results.NotFound();
            }

            return Results.Ok(product);
        }).WithName("GetProduct");
}
