namespace GeekShop.Api.Products.GetProducts;

internal static class GetProductsEndpoint
{
    public static void Map(RouteGroupBuilder group)
        => group.MapGet("/", async (GetProductsHandler handler, CancellationToken cancellationToken)
            => Results.Ok(await handler.HandleAsync(cancellationToken)));
}
